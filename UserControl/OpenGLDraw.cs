using System;
using Tao.FreeGlut;
using OpenGL;

namespace UserControl {

    class OpenGLDraw {
        private static int width = 400, height = 300;
        private static ShaderProgram program;
        
        // Elements for vertex buffer object
        private static VBO<Vector3> pyramid, cube; // Contains the vertices for each face
        private static VBO<Vector3> pyramidColor, cubeColor; // Vertices colors
        private static VBO<int> pyramidTriangles, cubeQuads; // How connect vertices

        private static System.Diagnostics.Stopwatch watch;
        private static float angle;
        public static Quaternion q, qy;

        public void View() {

            // create an OpenGL window
            Glut.glutInit();
            // Double bufferring for better drawing. Depth tells about color an depth position of pixels
            Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
            Glut.glutInitWindowSize(width, height);
            Glut.glutCreateWindow("OpenGL Tutorial");

            Glut.glutIdleFunc(OnRenderFrame);
            Glut.glutDisplayFunc(onDisplay);
            Glut.glutCloseFunc(onClose);

            // enable depth testing to ensure correct z-ordering of fragments
            Gl.Enable(EnableCap.DepthTest);

            // compile the shader program
            program = new ShaderProgram(VertexShader, FragmentShader);

            // set the view and projection matrix, which are static in this project
            program.Use();
            program["projection_matrix"].SetValue(Matrix4.CreatePerspectiveFieldOfView(0.6f, (float)width / height, 0.1f, 1000f));
            // Camera position at 10 bla
            program["view_matrix"].SetValue(Matrix4.LookAt(new Vector3(0, 0, 10), Vector3.Zero, Vector3.Up));

            /* Actually draw to the screen */
            // vertices: top, bottom-left, bottom-right
            // triangle = new VBO<Vector3>(new Vector3[] { new Vector3(0, 1, 0), new Vector3(-1, -1, 0), new Vector3(1, -1, 0) });
            pyramid = new VBO<Vector3>(new Vector3[] {
                new Vector3(0, 1, 0), new Vector3(-1, -1, 1), new Vector3(1, -1, 1),        // front face
                new Vector3(0, 1, 0), new Vector3(1, -1, 1), new Vector3(1, -1, -1),        // right face
                new Vector3(0, 1, 0), new Vector3(1, -1, -1), new Vector3(-1, -1, -1),      // back face
                new Vector3(0, 1, 0), new Vector3(-1, -1, -1), new Vector3(-1, -1, 1) });   // left face

            pyramidColor = new VBO<Vector3>(new Vector3[] {
                new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1),
                new Vector3(1, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0),
                new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1),
                new Vector3(1, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0) });

            pyramidTriangles = new VBO<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, BufferTarget.ElementArrayBuffer);

            // vertices: top-left, top-right, bottom-right, bottom-left
            // square = new VBO<Vector3>(new Vector3[] { new Vector3(-1, 1, 0), new Vector3(1, 1, 0), new Vector3(1, -1, 0), new Vector3(-1, -1, 0) });            
            // squareElements = new VBO<int>(new int[] { 0, 1, 2, 3}, BufferTarget.ElementArrayBuffer);
            cube = new VBO<Vector3>(new Vector3[] {
                new Vector3(1, 1, -1), new Vector3(-1, 1, -1), new Vector3(-1, 1, 1), new Vector3(1, 1, 1),
                new Vector3(1, -1, 1), new Vector3(-1, -1, 1), new Vector3(-1, -1, -1), new Vector3(1, -1, -1),
                new Vector3(1, 1, 1), new Vector3(-1, 1, 1), new Vector3(-1, -1, 1), new Vector3(1, -1, 1),
                new Vector3(1, -1, -1), new Vector3(-1, -1, -1), new Vector3(-1, 1, -1), new Vector3(1, 1, -1),
                new Vector3(-1, 1, 1), new Vector3(-1, 1, -1), new Vector3(-1, -1, -1), new Vector3(-1, -1, 1),
                new Vector3(1, 1, -1), new Vector3(1, 1, 1), new Vector3(1, -1, 1), new Vector3(1, -1, -1) });
            
            cubeColor = new VBO<Vector3>(new Vector3[] {
                new Vector3(0, 1, 0), new Vector3(0, 1, 0), new Vector3(0, 1, 0), new Vector3(0, 1, 0), 
                new Vector3(1, 0.5, 0), new Vector3(1, 0.5, 0), new Vector3(1, 0.5, 0), new Vector3(1, 0.5, 0),
                new Vector3(1, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 0), 
                new Vector3(1, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 0), 
                new Vector3(0, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 1), 
                new Vector3(1, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, 1) });
            
            cubeQuads = new VBO<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, BufferTarget.ElementArrayBuffer);

            watch = System.Diagnostics.Stopwatch.StartNew();

            Glut.glutMainLoop();
        }


        // Clear old frame (depth and color information)
        private static void OnRenderFrame(){

            watch.Stop();
            angle += (float)watch.ElapsedTicks / System.Diagnostics.Stopwatch.Frequency;
            if (angle > 2 * Math.PI)
                angle = 0.0f;
            watch.Restart();

            //q = new Quaternion((float)Math.Sin(angle) / (float)Math.Sqrt(2), (float)Math.Sin(angle) / (float)Math.Sqrt(2), 0f, (float)Math.Cos(angle));
            //qy = new Quaternion(0f, (float)Math.Sin(angle), 0f, (float)Math.Cos(angle));

            Gl.Viewport(0, 0, width, height);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            program.Use();

            ///* Draw triangle*/
            //// Create some translation to be applyed on the triangle, so it won't be overlaped with the square
            //program["model_matrix"].SetValue(QuatCreateRotation(qy) * Matrix4.CreateTranslation(new Vector3(-1.5f, 0, 0)));
            //// Give some color assigning pyramid_color to element vertexColor
            //Gl.BindBufferToShaderAttribute(pyramidColor, program, "vertexColor");
            //// Assign vertices and vertices_order to the vertex_position element of vertex_shader
            //uint vertexPositionIndex = (uint)Gl.GetAttribLocation(program.ProgramID, "vertexPosition");
            //Gl.EnableVertexAttribArray(vertexPositionIndex);
            //Gl.BindBuffer(pyramid);
            //// Pointer that moves every ucTime a vertex is drawn. Stride: number of bytes between adjacent vertices
            //// intPtr pointer is the starting point
            //Gl.VertexAttribPointer(vertexPositionIndex, pyramid.Size, pyramid.PointerType, true, 12, IntPtr.Zero);
            //Gl.BindBuffer(pyramidColor);
            //Gl.BindBuffer(pyramidTriangles);
            //Gl.DrawElements(BeginMode.Triangles, pyramidTriangles.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);


            /* Draw square using OpenGL built in functions */
            program["model_matrix"].SetValue(QuatCreateRotation(q) * Matrix4.CreateTranslation(new Vector3(0f, 0, 0)));
            Gl.BindBufferToShaderAttribute(cube, program, "vertexPosition");
            Gl.BindBuffer(cubeQuads);
            Gl.BindBufferToShaderAttribute(cubeColor, program, "vertexColor");
            Gl.DrawElements(BeginMode.Quads, cubeQuads.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);


            /* Draw tool using OpenGL built in functions */
            //program["model_matrix"].SetValue(Matrix4.CreateScaling(new Vector3(0.3, 0.3, 0.3)) * QuatCreateRotation(qy) * Matrix4.CreateTranslation(new Vector3(0, -1, 0)));
            //Gl.BindBufferToShaderAttribute(tool.Vertices, program, "vertexPosition");
            //Gl.BindBuffer(tool.FaceTop);
            //Gl.BindBufferToShaderAttribute(tool.Color, program, "vertexColor");
            //Gl.DrawElements(BeginMode.LineLoop, tool.FaceTop.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

            //program["model_matrix"].SetValue(Matrix4.CreateScaling(new Vector3(0.3, 0.3, 0.3)) * QuatCreateRotation(qy) * Matrix4.CreateTranslation(new Vector3(0, -1, 0)));
            //Gl.BindBufferToShaderAttribute(tool.Vertices, program, "vertexPosition");
            //Gl.BindBuffer(tool.FaceTop);
            //Gl.BindBufferToShaderAttribute(tool.Color, program, "vertexColor");
            //Gl.DrawElements(BeginMode.LineLoop, tool.FaceTop.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);


            // Brings work (back) buffer to screen to be drawn and puts old buffer back to work buffer
            Glut.glutSwapBuffers();

        }


        private static void onDisplay() { }

        private static void onClose() {
            // Dispose all resources ware created
            pyramid.Dispose();
            pyramidTriangles.Dispose();
            cube.Dispose();
            cubeQuads.Dispose();
            program.DisposeChildren = true;
            program.Dispose();
        }



        /* Vertex buffer object: contains information of vertices and which color and order to render them in
         *  VertexShader;
         *  FragmentShader; */

        /* VertexShader: Tells GPU how to manipulate vertices, draw fragments of pixel to screen;
         *  vertexPosition: Vertex that has to be transformed into screen space
         *  projection: Adds perspective;
         *  view: Manipulates camera position; 
         *  model: Translates from object position to world position (changes object reference axis) */
        public static string VertexShader = @"
in vec3 vertexPosition;
in vec3 vertexColor;

out vec3 color;

uniform mat4 projection_matrix;
uniform mat4 view_matrix;
uniform mat4 model_matrix;

void main(void)
{
    color = vertexColor;
    gl_Position = projection_matrix * view_matrix * model_matrix * vec4(vertexPosition, 1);
}
";
        /* FragmentShader: Set color for each fragment and does the drawing
         * color gets from vertex_shader a linear interpolation between the vertices color which will be used 
         * to fill the line color */
        public static string FragmentShader = @"
in vec3 color;

void main(void)
{
    gl_FragColor = vec4(color, 1);
}
";


        public static Matrix4 QuatCreateRotation(Quaternion q) {

            return new Matrix4(new Vector4(      q.x * q.x - q.y * q.y - q.z * q.z + q.w * q.w,
                                            2 * (q.x * q.y - q.z * q.w),
                                            2 * (q.x * q.z + q.y * q.w),
                                                 0.0f),
                               new Vector4( 2 * (q.x * q.y + q.z * q.w),
                                                -q.x * q.x + q.y * q.y - q.z * q.z + q.w * q.w,
                                            2 * (q.y * q.z - q.x * q.w),
                                                 0.0f),
                               new Vector4( 2 * (q.x * q.z - q.y * q.w),
                                            2 * (q.y * q.z + q.x * q.w),
                                                -q.x * q.x - q.y * q.y + q.z * q.z + q.w * q.w,
                                                 0.0f),
                               Vector4.UnitW);
        }
    }
}
