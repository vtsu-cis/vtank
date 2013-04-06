#region File Description
//-----------------------------------------------------------------------------
// GfxComponent.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;

namespace Renderer.SceneTools.ShadowMapping
{
    /// <summary>
    /// This class is designed to isolate graphics differences between the 
    /// Xbox and PC in these projects.  This class has logic for choosing a
    /// back buffer and depth stencils on PC.
    /// </summary>
    class GfxComponent : GameComponent
    {
        public GfxComponent(Game game, GraphicsDeviceManager graphics)
            : base(game)
        {
            graphics.PreferredBackBufferHeight = 720;
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreparingDeviceSettings +=
                new EventHandler<PreparingDeviceSettingsEventArgs>(
                    graphics_PreparingDeviceSettings);
        }

        void graphics_PreparingDeviceSettings(object sender, 
            PreparingDeviceSettingsEventArgs e)
        {
            int quality = 0;
            GraphicsAdapter adapter = e.GraphicsDeviceInformation.Adapter;
            SurfaceFormat format = adapter.CurrentDisplayMode.Format;
            DisplayMode currentmode = adapter.CurrentDisplayMode;

            PresentationParameters pp = 
                e.GraphicsDeviceInformation.PresentationParameters;

#if XBOX
            pp.MultiSampleQuality = 0;
            pp.MultiSampleType =
                MultiSampleType.FourSamples;
            pp.BackBufferWidth = 1280;
            pp.BackBufferHeight = 720;
            pp.BackBufferFormat = SurfaceFormat.Bgr32;
            pp.AutoDepthStencilFormat = DepthFormat.Depth24Stencil8Single;
            pp.EnableAutoDepthStencil = true;
            return;
#endif

            // Set a window size compatible with the current screen
            if (currentmode.Width < 800)
            {
                pp.BackBufferWidth = 640;
                pp.BackBufferHeight = 480;
            }
            else if (currentmode.Width < 1024)
            {
                pp.BackBufferWidth = 800;
                pp.BackBufferHeight = 600;
            }
            else if (currentmode.Width < 1280)
            {
                pp.BackBufferWidth = 1024;
                pp.BackBufferHeight = 768;
            }
            else // Xbox, or a PC with a big screen
            {
                pp.BackBufferWidth = 1280;
                pp.BackBufferHeight = 720;
            }

            // Xbox 360 and most PCs support FourSamples/0 (4x) 
            // and TwoSamples/0 (2x) antialiasing.
            // Check for 4xAA
            if (adapter.CheckDeviceMultiSampleType(DeviceType.Hardware, format,
                false, MultiSampleType.FourSamples, out quality))
            {
                // even if a greater quality is returned, we only want quality 0
                pp.MultiSampleQuality = 0;
                pp.MultiSampleType = MultiSampleType.FourSamples;
            }
            // Check for 2xAA
            else if (adapter.CheckDeviceMultiSampleType(DeviceType.Hardware, 
                format, false, MultiSampleType.TwoSamples, out quality))
            {
                // even if a greater quality is returned, we only want quality 0
                pp.MultiSampleQuality = 0;
                pp.MultiSampleType = MultiSampleType.TwoSamples;
            }

            // Does this video card support Depth24Stencil8Single 
            // for the back buffer?
            if (adapter.CheckDeviceFormat(DeviceType.Hardware, 
                adapter.CurrentDisplayMode.Format, TextureUsage.None,
                QueryUsages.None, ResourceType.RenderTarget, 
                DepthFormat.Depth24Stencil8Single))
            {
                // if so, let's use that
                pp.AutoDepthStencilFormat = DepthFormat.Depth24Stencil8Single;
                pp.EnableAutoDepthStencil = true;
            }
            return;
        }
        public static RenderTarget2D CloneRenderTarget(
            RenderTarget2D original, int numberLevels)
        {
            return new RenderTarget2D(original.GraphicsDevice,
                original.Width, original.Height, numberLevels, original.Format,
                original.MultiSampleType, original.MultiSampleQuality);
        }
        public static RenderTarget2D CloneRenderTarget(
            GraphicsDevice device, int numberLevels)
        {
            return new RenderTarget2D(device,
                device.PresentationParameters.BackBufferWidth,
                device.PresentationParameters.BackBufferHeight,
                numberLevels,
                device.DisplayMode.Format,
                device.PresentationParameters.MultiSampleType,
                device.PresentationParameters.MultiSampleQuality
            );
        }
        public static bool CheckTextureSize(int width, int height, 
            out int newwidth, out int newheight)
        {
            bool retval = false;

            GraphicsDeviceCapabilities Caps;
            Caps = GraphicsAdapter.DefaultAdapter.GetCapabilities(
                DeviceType.Hardware);

            if (Caps.TextureCapabilities.RequiresPower2)
            {
                retval = true;  // Return true to indicate the numbers changed

                // Find the nearest base two log of the current width, 
                // and go up to the next integer                
                double exp = Math.Ceiling(Math.Log(width)/Math.Log(2));
                // and use that as the exponent of the new width
                width = (int)Math.Pow(2, exp);
                // Repeat the process for height
                exp = Math.Ceiling(Math.Log(height)/Math.Log(2));
                height = (int)Math.Pow(2, exp);
            }
            if (Caps.TextureCapabilities.RequiresSquareOnly)
            {
                retval = true;  // Return true to indicate numbers changed
                width = Math.Max(width, height);
                height = width;
            }

            newwidth = Math.Min(Caps.MaxTextureWidth, width);
            newheight = Math.Min(Caps.MaxTextureHeight, height);
            return retval;
        }

        public static RenderTarget2D CreateRenderTarget(GraphicsDevice device, 
            int numberLevels, SurfaceFormat surface)
        {
            MultiSampleType type = 
                device.PresentationParameters.MultiSampleType;

            // If the card can't use the surface format
            if (!GraphicsAdapter.DefaultAdapter.CheckDeviceFormat(
                DeviceType.Hardware,
                GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Format, 
                TextureUsage.None,
                QueryUsages.None, 
                ResourceType.RenderTarget, 
                surface))
            {
                // Fall back to current display format
                surface = device.DisplayMode.Format;
            }
            // Or it can't accept that surface format 
            // with the current AA settings
            else if (!GraphicsAdapter.DefaultAdapter.CheckDeviceMultiSampleType(
                DeviceType.Hardware, surface, 
                device.PresentationParameters.IsFullScreen, type))
            {
                // Fall back to no antialiasing
                type = MultiSampleType.None;
            }

            int width, height;

            // See if we can use our buffer size as our texture
            CheckTextureSize(device.PresentationParameters.BackBufferWidth, 
                device.PresentationParameters.BackBufferHeight,
                out width, out height);

            // Create our render target
            return new RenderTarget2D(device,
                width, height, numberLevels, surface,
                type, 0);

        }
        public static DepthStencilBuffer CreateDepthStencil(
            RenderTarget2D target)
        {
            return new DepthStencilBuffer(target.GraphicsDevice, target.Width,
                target.Height, target.GraphicsDevice.DepthStencilBuffer.Format,
                target.MultiSampleType, target.MultiSampleQuality);
        }
        public static DepthStencilBuffer CreateDepthStencil(
            RenderTarget2D target, DepthFormat depth)
        {
            if (GraphicsAdapter.DefaultAdapter.CheckDepthStencilMatch(
                DeviceType.Hardware, 
                GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Format, 
                target.Format, 
                depth))
            {
                return new DepthStencilBuffer(target.GraphicsDevice, 
                    target.Width, target.Height, depth, 
                    target.MultiSampleType, target.MultiSampleQuality);
            }
            else
                return CreateDepthStencil(target);
        }
    }
}
