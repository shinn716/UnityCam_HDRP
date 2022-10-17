using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

public class UnityCamSender : MonoBehaviour
{
    internal const string DllName = "UnityWebcam";

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    extern static private IntPtr CreateTextureWrapper();

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    extern static private void DeleteTextureWrapper(IntPtr w);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    extern static private bool SendTexture(IntPtr w, IntPtr textureID);


    [SerializeField] private RenderTexture rt;
    [SerializeField] private bool Flip = false;

    private IntPtr _instance;
    private TextureWrapper _wrapper;
    private OffscreenProcessor _BlitterProcessor;
    private bool isRunning = false;

    // Start is called before the first frame update
    void Start()
    {
        //Init UnityWebCamera plugin
        _instance = CreateTextureWrapper();
        _BlitterProcessor = new OffscreenProcessor("UnityCam/Image/Blitter");
        _wrapper = new TextureWrapper();

        isRunning = true;
        CaptureLoop();
    }

    private void OnDisable()
    {
        isRunning = false;
    }

    async private void CaptureLoop()
    {
        while (isRunning)
        {
            RenderImage(rt);
            //Debug.Log("func: In WhileTrue loop");
            await Task.Yield();
        }
        //Debug.Log("func: After whileTrue loop");
    }

    private void RenderImage(RenderTexture source)
    {
        Texture tex = source;

        if (Flip)
            tex = _BlitterProcessor.ProcessTexture(tex, 0);
        else
            tex = _BlitterProcessor.ProcessTexture(tex, 1);

        _wrapper.ConvertTexture(tex);
        tex = _wrapper.WrappedTexture;

        //Send the rendered image to the plugin 
        SendTexture(_instance, tex.GetNativeTexturePtr());

        //if (BlitLocaly)
        //    Graphics.Blit(source, destination);
    }

}
