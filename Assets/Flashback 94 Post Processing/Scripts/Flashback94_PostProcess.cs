//////////////////////////////////////////////////////////////////////////////////////////
//																						//
// Flashback '94 Shader Pack for Unity 3D												//
// © 2018 George Kokoris          														//
//																						//
// Post-process script for scaling the framebuffer and quantizing colors                //
// Only for use with the 'Hidden/Flashback 94/Color Quantize' shader                    //
//                                                                                      //
// Modified by Fire Plant Games to work with the new post procesing stack               //
//////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[System.Serializable]
[PostProcess (typeof (Flashback94_PostProcessRenderer), PostProcessEvent.AfterStack, "Custom/Flashback Shader")]
public sealed class Flashback94_PostProcess : PostProcessEffectSettings
{
    // Should we filter color?
    public BoolParameter enableColorFiltering = new BoolParameter {value = true};
    // Bits per color channel
    public FloatParameter bitsPerChannel = new FloatParameter {value = 8};

    // Width and height for absolute downsampling
    public IntParameter downsampleAbsoluteWidth = new IntParameter { value = 320 };
    public IntParameter downsampleAbsoluteHeight = new IntParameter {value = 240};

    // Enable/disable antialiasing when blitting
    public BoolParameter downsampleAntialiasing = new BoolParameter {value = false};
}

public sealed class Flashback94_PostProcessRenderer : PostProcessEffectRenderer <Flashback94_PostProcess>
{
    public Material tempMaterial;

    public override void Init()
    {
        base.Init();
        // set up our material
        tempMaterial = new Material(Shader.Find ("Hidden/Flashback 94/Color Quantize"));
        tempMaterial.hideFlags = HideFlags.DontSave;
    }

    public override void Render(PostProcessRenderContext context)
    {
        // Should we filter the color?
        tempMaterial.SetFloat ("_ShouldStep", settings.enableColorFiltering ? 1.0f : -1.0f);

        // Set the number of color steps in the shader
        if (settings.enableColorFiltering)
            tempMaterial.SetFloat ("_ColorSteps", Mathf.Pow (2f, settings.bitsPerChannel));

        // set our width/height
        var bufWidth = settings.downsampleAbsoluteWidth;
        var bufHeight = settings.downsampleAbsoluteHeight;
        
        // Create a temporary texture using our buffer width/height and filter it by point
        var buffer = RenderTexture.GetTemporary(bufWidth, bufHeight, 0);
        buffer.filterMode = FilterMode.Point;

        // Create a new render texture (this will be our source) with our current width/height. Set it's filter mode appropriately
        RenderTexture t = RenderTexture.GetTemporary (context.width, context.height, 0);
        t.filterMode = settings.downsampleAntialiasing ? FilterMode.Bilinear : FilterMode.Point;

        // blit our source into the new texture
        context.command.Blit (context.source, t);

        // blit our new texture into our buffer, passing it through the tempMaterial
        context.command.Blit (t, buffer, tempMaterial);

        // blit the buffer into the destination.
        context.command.Blit (buffer, context.destination);

        // cleanup.
        RenderTexture.ReleaseTemporary(buffer);
        RenderTexture.ReleaseTemporary(t);
    }
}