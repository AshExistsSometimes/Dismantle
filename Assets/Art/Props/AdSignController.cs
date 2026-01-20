using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Scrolls ads by stacking two textures into one runtime texture and scrolling UVs.
/// Keeps everything perfectly clipped inside the sign quad.
/// Supports broken screens.
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
public class AdSignUVScroller : MonoBehaviour
{
    public enum ScrollDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    [Header("Playlist")]
    public List<Texture2D> adTextures = new List<Texture2D>();
    public bool shufflePlaylist = true;

    [Header("Playback")]
    public float displayTime = 4f;
    public float scrollDuration = 1f;
    public float randomStartDelay = 3f;
    public ScrollDirection scrollDirection = ScrollDirection.Down;

    [Header("Broken Screen")]
    [Range(0f, 1f)]
    public float brokenChance = 0.15f;
    public Texture2D brokenScreenTexture;

    private List<Texture2D> runtimePlaylist = new();
    private int currentIndex;

    private MeshRenderer meshRenderer;
    private Material runtimeMaterial;

    private Texture2D stackedTexture;
    private Coroutine playbackRoutine;

    private bool isBroken;

    ////////////////////////////////////////////////////////
    // Unity

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        runtimeMaterial = meshRenderer.material; // unique instance per sign
    }

    private void OnEnable()
    {
        // Make sure this sign has its own material instance
        if (runtimeMaterial == null)
            runtimeMaterial = meshRenderer.material;

        RollBrokenState();

        if (isBroken)
        {
            ApplyBrokenScreen();
            return;
        }

        if (adTextures.Count == 0)
            return;

        BuildRuntimePlaylist();
        playbackRoutine = StartCoroutine(PlaybackLoop());
    }

    private void OnDisable()
    {
        if (playbackRoutine != null)
            StopCoroutine(playbackRoutine);
    }

    ////////////////////////////////////////////////////////
    // Broken Screen Logic

    private void RollBrokenState()
    {
        isBroken = Random.value < brokenChance;
    }

    private void ApplyBrokenScreen()
    {
        if (brokenScreenTexture == null)
        {
            Debug.LogWarning($"AdSignUVScroller: '{name}' is broken but has no broken texture assigned.");
            return;
        }

        runtimeMaterial.mainTexture = brokenScreenTexture;
        runtimeMaterial.mainTextureScale = Vector2.one;
        runtimeMaterial.mainTextureOffset = Vector2.zero;
    }

    ////////////////////////////////////////////////////////
    // Playlist

    private void BuildRuntimePlaylist()
    {
        runtimePlaylist.Clear();
        runtimePlaylist.AddRange(adTextures);

        if (shufflePlaylist)
            Shuffle(runtimePlaylist);

        currentIndex = Random.Range(0, runtimePlaylist.Count);
    }

    ////////////////////////////////////////////////////////
    // Playback Loop

    private IEnumerator PlaybackLoop()
    {
        if (randomStartDelay > 0f)
            yield return new WaitForSeconds(Random.Range(0f, randomStartDelay));

        SetSingleTexture(runtimePlaylist[currentIndex]);

        while (true)
        {
            yield return new WaitForSeconds(displayTime);
            AdvanceAd();
        }
    }

    private void AdvanceAd()
    {
        int nextIndex;
        do
        {
            nextIndex = Random.Range(0, runtimePlaylist.Count);
        } while (nextIndex == currentIndex && runtimePlaylist.Count > 1);

        Texture2D current = runtimePlaylist[currentIndex];
        Texture2D next = runtimePlaylist[nextIndex];

        currentIndex = nextIndex;

        StartCoroutine(ScrollTransition(current, next));
    }


    ////////////////////////////////////////////////////////
    // Texture Interface (SINGLE ASSIGNMENT POINT)

    private void SetSingleTexture(Texture2D texture)
    {
        runtimeMaterial.mainTexture = texture;
        runtimeMaterial.mainTextureScale = Vector2.one;
        runtimeMaterial.mainTextureOffset = Vector2.zero;
    }

    private void SetScrollTexture(Texture2D stacked)
    {
        runtimeMaterial.mainTexture = stacked;
    }

    ////////////////////////////////////////////////////////
    // Scrolling

    private IEnumerator ScrollTransition(Texture2D current, Texture2D next)
    {
        BuildStackedTexture(current, next);
        SetScrollTexture(stackedTexture);

        Vector2 scale = GetUVScale();
        runtimeMaterial.mainTextureScale = scale;
        Vector2 startOffset = GetStartUVOffset();
        Vector2 endOffset = GetEndUVOffset();

        runtimeMaterial.mainTextureScale = GetUVScale();
        runtimeMaterial.mainTextureOffset = startOffset;

        float t = 0f;
        while (t < scrollDuration)
        {
            t += Time.deltaTime;
            float lerp = t / scrollDuration;
            runtimeMaterial.mainTextureOffset = Vector2.Lerp(startOffset, endOffset, lerp);
            yield return null;
        }
    }

    ////////////////////////////////////////////////////////
    // Texture Stacking

    private void BuildStackedTexture(Texture2D current, Texture2D next)
    {
        if (!current.isReadable || !next.isReadable)
            return;

        bool horizontal = scrollDirection == ScrollDirection.Left || scrollDirection == ScrollDirection.Right;

        int width = horizontal ? current.width * 2 : current.width;
        int height = horizontal ? current.height : current.height * 2;

        if (stackedTexture == null || stackedTexture.width != width || stackedTexture.height != height)
        {
            stackedTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            stackedTexture.filterMode = FilterMode.Point;
            stackedTexture.wrapMode = TextureWrapMode.Clamp;
        }

        Color[] clear = new Color[width * height];
        stackedTexture.SetPixels(clear);

        if (horizontal)
        {
            if (scrollDirection == ScrollDirection.Left)
            {
                // Current on left, Next on right
                stackedTexture.SetPixels(0, 0, current.width, current.height, current.GetPixels());
                stackedTexture.SetPixels(current.width, 0, next.width, next.height, next.GetPixels());
            }
            else // Right
            {
                // Current on right, Next on left
                stackedTexture.SetPixels(0, 0, next.width, next.height, next.GetPixels());
                stackedTexture.SetPixels(next.width, 0, current.width, current.height, current.GetPixels());
            }
        }
        else
        {
            if (scrollDirection == ScrollDirection.Down)
            {
                // Current at bottom, Next on top
                stackedTexture.SetPixels(0, 0, current.width, current.height, current.GetPixels());
                stackedTexture.SetPixels(0, current.height, next.width, next.height, next.GetPixels());
            }
            else // Up
            {
                // Current at top, Next on bottom
                stackedTexture.SetPixels(0, next.height, current.width, current.height, current.GetPixels());
                stackedTexture.SetPixels(0, 0, next.width, next.height, next.GetPixels());
            }
        }

        stackedTexture.Apply();
    }


    ////////////////////////////////////////////////////////
    // UV Helpers

    private Vector2 GetUVScale()
    {
        if (scrollDirection == ScrollDirection.Left || scrollDirection == ScrollDirection.Right)
            return new Vector2(0.5f, 1f); // show half width
        return new Vector2(1f, 0.5f);     // show half height
    }

    private Vector2 GetStartUVOffset()
    {
        switch (scrollDirection)
        {
            case ScrollDirection.Left: return new Vector2(0f, 0f);      // current left
            case ScrollDirection.Right: return new Vector2(0.5f, 0f);    // current right
            case ScrollDirection.Down: return new Vector2(0f, 0f);      // current at bottom
            case ScrollDirection.Up: return new Vector2(0f, 0.5f);    // current at top
        }
        return Vector2.zero;
    }

    private Vector2 GetEndUVOffset()
    {
        switch (scrollDirection)
        {
            case ScrollDirection.Left: return new Vector2(0.5f, 0f);    // next comes from right
            case ScrollDirection.Right: return new Vector2(0f, 0f);      // next comes from left
            case ScrollDirection.Down: return new Vector2(0f, 0.5f);    // next comes from top
            case ScrollDirection.Up: return new Vector2(0f, 0f);      // next comes from bottom
        }
        return Vector2.zero;
    }



    ////////////////////////////////////////////////////////
    // Utility

    private void Shuffle(List<Texture2D> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
