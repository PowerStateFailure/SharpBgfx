using System;
using Common;
using SharpBgfx;

static class Program {
    static void Main () {
        // create a UI thread and kick off a separate render thread
        var sample = new Sample("Mesh", 1280, 720);
        sample.Run(RenderThread);
    }

    static unsafe void RenderThread (Sample sample) {
        // initialize the renderer
        Bgfx.Init();
        Bgfx.Reset(sample.WindowWidth, sample.WindowHeight, ResetFlags.Vsync);

        // enable debug text
        Bgfx.SetDebugFeatures(DebugFeatures.DisplayText);

        // set view 0 clear state
        Bgfx.SetViewClear(0, ClearTargets.Color | ClearTargets.Depth, 0x303030ff);

        // create vertex and index buffers
        var mesh = ResourceLoader.LoadMesh("bunny.bin");
        var renderGroup = new RenderStateGroup(RenderState.Default, uint.MaxValue, StencilFlags.None, StencilFlags.None);
        
        // create uniform
        var u_time = new Uniform("u_time", UniformType.Vector4);

        // load shaders
        var program = ResourceLoader.LoadProgram("vs_mesh", "fs_mesh");

        // start the frame clock
        var clock = new Clock();
        clock.Start();

        // main loop
        while (sample.ProcessEvents(ResetFlags.Vsync)) {
            // set view 0 viewport
            Bgfx.SetViewRect(0, 0, 0, sample.WindowWidth, sample.WindowHeight);

            // view transforms
            var viewMatrix = Matrix4x4.CreateLookAt(new Vector3(0.0f, 0.0f, -2.5f), new Vector3(0.0f, 1.0f, 0.0f), Vector3.UnitY);
            var projMatrix = Matrix4x4.CreatePerspectiveFieldOfView((float)Math.PI / 3, (float)sample.WindowWidth / sample.WindowHeight, 0.1f, 100.0f);
            Bgfx.SetViewTransform(0, &viewMatrix.M11, &projMatrix.M11);

            // make sure view 0 is cleared if no other draw calls are submitted
            Bgfx.Touch(0);

            // tick the clock
            var elapsed = clock.Frame();
            var time = clock.TotalTime();
            
            // aplly uniform
            Bgfx.SetUniform(u_time, &time);

            // write some debug text
            Bgfx.DebugTextClear();
            Bgfx.DebugTextWrite(0, 1, DebugColor.White, DebugColor.Blue, "SharpBgfx/Samples/04-Mesh");
            Bgfx.DebugTextWrite(0, 2, DebugColor.White, DebugColor.Cyan, "Description: Loading meshes.");
            Bgfx.DebugTextWrite(0, 3, DebugColor.White, DebugColor.Cyan, "Frame: {0:F3} ms", elapsed * 1000);

            var matrix = Matrix4x4.CreateRotationY(time * 0.37f);
                
            // submit primitives
            mesh.Submit(0, program, &matrix, renderGroup, null);

            // advance to the next frame. Rendering thread will be kicked to
            // process submitted rendering primitives.
            Bgfx.Frame();
        }

        // clean up
        mesh.Dispose();
        u_time.Dispose();
        program.Dispose();
        Bgfx.Shutdown();
    }
}