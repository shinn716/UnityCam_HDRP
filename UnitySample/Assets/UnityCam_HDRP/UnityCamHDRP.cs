using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class UnityCamHDRP : CustomPass
{
    internal const string DllName = "UnityWebcam";

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    extern static private IntPtr CreateTextureWrapper();

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    extern static private void DeleteTextureWrapper(System.IntPtr w);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    extern static private bool SendTexture(System.IntPtr w, System.IntPtr textureID);

    [SerializeField] private Camera virtualCam;
    [SerializeField] private Texture resultTexture;
    [SerializeField] private bool Flip = false;

    public Texture ResultTexture { get { return resultTexture; } }

    private IntPtr _instance;
    private TextureWrapper _wrapper;
    private OffscreenProcessor _BlitterProcessor;
    private RenderTexture buffer;

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        if (virtualCam == null)
            return;

        //Init UnityWebCamera plugin
        _instance = CreateTextureWrapper();
        _BlitterProcessor = new OffscreenProcessor("UnityCam/Image/Blitter");
        _wrapper = new TextureWrapper();

        buffer = new RenderTexture(1280, 720, 0);
        buffer.hideFlags = HideFlags.DontSave;
        virtualCam.targetTexture = buffer;
    }

    protected override void Execute(CustomPassContext ctx)
    {
        if (virtualCam == null)
            return;

        if (virtualCam.targetTexture == null)
            return;

        if (ctx.hdCamera.camera.cameraType == CameraType.Game)
            RenderImage(virtualCam.targetTexture);
    }


    protected override void Cleanup() 
    {

    }

    protected void RenderImage(Texture _source)
    {
        if (Flip)
            _source = _BlitterProcessor.ProcessTexture(_source, 0);
        else
            _source = _BlitterProcessor.ProcessTexture(_source, 1);

        _wrapper.ConvertTexture(_source);
        _source = _wrapper.WrappedTexture;
        resultTexture = _source;

        //Send the rendered image to the plugin 
        SendTexture(_instance, _source.GetNativeTexturePtr());
    }
}
