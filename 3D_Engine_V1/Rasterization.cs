using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Security.Cryptography.Pkcs;
using System.Drawing.Drawing2D;

namespace _3D_3ngine_V1
{
    public class Rasterization
    {
        float viewport_size = 1;
        float projection_plane_z = 1;
        Bitmap canvas;
        Graphics canvas_context;
        int canvas_width;
        int canvas_height;
        Camera camera = new Camera(new Vertex(3, 0, 0), Mtx.RotX(0));
        private float[,] zBuffer;

        public Bitmap Canvas
        {
            get { return canvas; }
        }

        public Rasterization(Size size, Vertex vert, Vertex traslate, float z, Instance[] instances)
        {
            zBuffer = new float[size.Width, size.Height];
            Init(size, z, vert, traslate, instances);
        }

        private void Init(Size size, float z, Vertex vert, Vertex translate, Instance[] instances)
        {

            canvas = new Bitmap(size.Width, size.Height);
            canvas_context = Graphics.FromImage(canvas);
            canvas_width = canvas.Width;
            canvas_height = canvas.Height;

            ComputePlanes(90);//degrees
                              // instances
            Render(instances, translate);

            // here is not implemented the FOV in the view only the planes at that angle
            // to implement the FOV in the canvas we need to create also that matrix
            // of the camera with that same angle
        }

        public void InitializeZBuffer(int width, int height)
        {
            zBuffer = new float[canvas_width, canvas_height];
            for (int x = 0; x < canvas_width; x++)
            {
                for (int y = 0; y < canvas_height; y++)
                {
                    zBuffer[x, y] = float.MaxValue;
                }
            }
        }

        // The PutPixel() function.
        public void PutPixel(int x, int y, Color color)
        {
            x = canvas_width / 2 + x;
            y = canvas_height / 2 - y - 1;

            if (x < 0 || x >= canvas_width || y < 0 || y >= canvas_height)
            {
                return;
            }

            canvas.SetPixel(x, y, color);
        }

        List<float> Interpolate(int i0, float d0, int i1, float d1)
        {
            if (i0 == i1)
            {
                return new List<float> { d0 };
            }

            List<float> values = new List<float>();
            float a = (d1 - d0) / (i1 - i0);
            float d = d0;
            for (var i = i0; i <= i1; i++)
            {
                values.Add(d);
                d += a;
            }

            return values;
        }

        public void Swap(ref Vertex a, ref Vertex b)
        {
            Vertex temp = a;
            a = b;
            b = temp;
        }

        public void DrawFilledTriangle(Vertex p0, Vertex p1, Vertex p2, Color color)
        {

            List<float> xLeft;
            List<float> xRight;
            List<float> zLeft, zRight;
            //int z0 = (int)p0.z;
            //int z1 = (int)p1.z;
            //int z2 = (int)p2.z;
            //SortBy(p0, p1, p2); //Sort the points so that y0 <= y1 z= y2
            SortBy(ref p0, ref p1, ref p2);

            //Compute the x coordinates of the tirangles edges
            var x01 = Interpolate((int)p0.y, p0.x, (int)p1.y, p1.x);
            var x12 = Interpolate((int)p1.y, p1.x, (int)p2.y, p2.x);
            var x02 = Interpolate((int)p0.y, p0.x, (int)p2.y, p2.x);

            var z01 = Interpolate((int)p0.y, p0.z, (int)p1.y, p1.z);
            var z12 = Interpolate((int)p1.y, p1.z, (int)p2.y, p2.z);
            var z02 = Interpolate((int)p0.y, p0.z, (int)p2.y, p2.z);


            //var ys = Interpolate((int)p0.x, p0.y, (int)p1.x, p1.y); // ys = Interpolate(x0,y0,x1,y1)
            //x012 = new List<float>();
            //z012 = new List<float>();

            x01.RemoveAt(x01.Count - 1); //Remove the last value from the list because its repeated in x12
            z01.RemoveAt(z01.Count - 1);

            //Concatenate the short sides 
            var x012 = x01.Concat(x12).ToList(); //List<int> combinedList = list1.Concat(list2).ToList();
            var z012 = z01.Concat(z12).ToList();
            //Determine which is left and which is right

            //var val = (int)x02.Count / 2;
            int m = (int)Math.Floor((double)(x02.Count / 2));
            if (x02[m] < x012[m])
            {
                    xLeft = x02;
                    xRight = x012;

                    zLeft = z02;
                    zRight = z012;
            }
            else
            {
                    xLeft = x012;
                    xRight = x02;

                    zLeft = z012;
                    zRight = z02;
            }

            //Draw the horizontal segments
            for (int y = (int)p0.y; y < p2.y; y++)
            {
                int xStart = (int)xLeft[y - (int)p0.y];
                int xEnd = (int)xRight[y - (int)p0.y];
                for (int x = xStart; x < xEnd; x++)
                {
                    if (x >= 0 && x < zBuffer.GetLength(0) && y >= 0 && y < zBuffer.GetLength(1))
                    {
                        // Interpolate z values for each horizontal segment
                        float zSegment = Interpolate(xStart, zLeft[y - (int)p0.y], xEnd, zRight[y - (int)p0.y])[x - xStart];
                        // Calculate the depth of the current pixel (z)
                        //float denom = ((p1.y - p2.y) * (p0.x - p2.x) + (p2.x - p1.x) * (p0.y - p2.y));
                        //float u = ((p1.y - p2.y) * (x - p2.x) + (p2.x - p1.x) * (y - p2.y)) / denom;
                        //float v = ((p2.y - p0.y) * (x - p2.x) + (p0.x - p2.x) * (y - p2.y)) / denom;
                        //float w = 1 - u - v;
                        //float bc = u * p0.z + v * p1.z + w * p2.z;


                        // Replace the depth calculation for the current pixel (z) with the interpolated zSegment value
                        float z = zSegment;
                        // Compare the depth value with the Z-buffer
                        if (z < zBuffer[x, y])
                        {
                            // Update the Z-buffer
                            zBuffer[x, y] = z;
                            // Draw the pixel
                            PutPixel(x, y, color);
                        }
                    }
                }
            }

            //for (int y = (int)p0.y; y <= p2.y; y++)
            //{
            //    for (int x = (int)xLeft[y - (int)p0.y]; x <= xRight[y - (int)p0.y]; x++)
            //    {
            //        PutPixel(x, y, color);
            //    }
            //}
        }

        public void SortBy(ref Vertex P0, ref Vertex P1, ref Vertex P2)
        {
            if (P1.y < P0.y)
            {
                Swap(ref P1, ref P0);
            }
            if (P2.y < P0.y)
            {
                Swap(ref P2, ref P0);
            }
            if (P2.y < P1.y)
            {
                Swap(ref P2, ref P1);
            }
        }

        public void DrawWireframeTriangle(Vertex p0, Vertex p1, Vertex p2, Color color)
        {
            DrawLine(p0, p1, color);
            DrawLine(p1, p2, color);
            DrawLine(p0, p2, color);
        }

        public void DrawLine(Vertex p1, Vertex p2, Color c)
        {
            var dx = p2.X - p1.X;
            var dy = p2.Y - p1.Y;

            if (Math.Abs(dx) > Math.Abs(dy))
            {
                if (dx < 0) Swap(ref p1, ref p2);
                var ys = Interpolate((int)p1.X, p1.Y, (int)p2.X, p2.Y);
                for (var x = (int)p1.X; x <= p2.X; x++)
                {
                    PutPixel(x, (int)ys[x - (int)p1.X], c);
                }
            }
            else
            {
                if (dy < 0) Swap(ref p1, ref p2);
                var xs = Interpolate((int)p1.Y, p1.X, (int)p2.Y, p2.X);
                for (var y = (int)p1.Y; y <= p2.Y; y++)
                {
                    PutPixel((int)xs[y - (int)p1.Y], y, c);
                }
            }
        }

        public void ComputePlanes(float fov)
        {
            Vertex left, right, bottom, top;
            float aspect = 1f;
            float near = 0.1f;

            float tanFov = (float)Math.Tan(fov * 0.5f * Math.PI / 180f);
            float height = 2f * tanFov * near;
            float width = height * aspect;

            // Left plane
            float sx = 1f * (width / 2f);
            float sy = 0f;
            float sz = near;

            left = new Vertex(sx, sy, sz);
            left = left.Normalize();

            // Right plane
            sx = -width / 2f;
            sy = 0f;
            sz = near;
            right = new Vertex(sx, sy, sz);
            right = right.Normalize();

            // Bottom plane
            sx = 0f;
            sy = -1f * (height / 2f);
            sz = near;
            bottom = new Vertex(sx, sy, sz);
            bottom = bottom.Normalize();

            // Top plane
            sx = 0f;
            sy = height / 2f;
            sz = near;
            top = new Vertex(sx, sy, sz);
            top = top.Normalize();

            camera.clipping_planes.Add(new Plane(new Vertex(0, 0, 1), 0));   // near
            camera.clipping_planes.Add(new Plane(left, 0));  // left
            camera.clipping_planes.Add(new Plane(right, 0));  // right
            camera.clipping_planes.Add(new Plane(top, 0));  // top
            camera.clipping_planes.Add(new Plane(bottom, 0));  // bottom
        }

        public void Render(Instance[] instances, Vertex translate)
        {

            RenderScene(camera, instances, translate);
        }

        private void RenderModel(Instance instance, Mtx transform)
        {
            List<Vertex> projected = new List<Vertex>();
            Model model = instance.model;

            for (int i = 0; i < model.vertices.Length; i++)
            {
                projected.Add(ProjectVertex(transform * model.vertices[i]));
            }

            for (int i = 0; i < model.triangles.Length; i++)
            {
                RenderTriangle(model.triangles[i], projected);
            }
        }

        public void RenderScene(Camera camera, Instance[] instances, Vertex translate) //Optimized 
        {
            Mtx cameraMatrix;
            Mtx transform;
            InitializeZBuffer(canvas_width, canvas_height);
            cameraMatrix = (camera.orientation.Transposed()) * Mtx.MakeTranslationMatrix(-camera.position);
            for (int i = 0; i < instances.Length; i++)
            {
                //InitializeZBuffer(canvas_width, canvas_height);
                instances[i].position = translate;
                transform = (cameraMatrix * instances[i].transform);
                RenderModel(instances[i], transform);
            }
        }

        public void RenderTriangle(Triangle triangle, List<Vertex> projected)
        {

            if (getNormal(projected[triangle.v0], projected[triangle.v1], projected[triangle.v2]).Z < 0)
            {
                DrawFilledTriangle(projected[triangle.v0], projected[triangle.v1], projected[triangle.v2], Color.Blue);

                DrawWireframeTriangle(projected[triangle.v0], projected[triangle.v1], projected[triangle.v2], Color.Red);
            }

        }

        // Converts 2D viewport coordinates to 2D canvas coordinates.
        Vertex ViewportToCanvas(Vertex p2d)
        {
            float vW = (float)canvas.Width / canvas.Height;
            return new Vertex((p2d.x * canvas.Width / vW), (p2d.y * canvas.Height / viewport_size), p2d.z);
        }

        Vertex ProjectVertex(Vertex v)
        {
            // return ViewportToCanvas(new Vertex(v.x * projection_plane_z / v.z, v.y * projection_plane_z / v.z, 0));
            return ViewportToCanvas(new Vertex(v.x * projection_plane_z / v.z, v.y * projection_plane_z / v.z, v.z));
        }

        public Vertex getNormal(Vertex p0, Vertex p1, Vertex p2)
        {
            // Calculate the normal of the triangle formed by p0, p1, p2
            Vertex normal = new Vertex();

            // Calculate the edge vectors
            Vertex edge1 = p1 - p0;
            Vertex edge2 = p2 - p0;

            // Calculate the cross product of the edge vectors
            normal.x = edge1.y * edge2.z - edge1.z * edge2.y;
            normal.y = edge1.z * edge2.x - edge1.x * edge2.z;
            normal.z = edge1.x * edge2.y - edge1.y * edge2.x;

            // Normalize the normal vector
            float length = (float)Math.Sqrt(normal.x * normal.x + normal.y * normal.y + normal.z * normal.z);
            normal.x /= length;
            normal.y /= length;
            normal.z /= length;

            return normal;
        }

        private List<Triangle> ClipTriangle(Triangle triangle, Plane plane, List<Triangle> triangles, List<Vertex> vertices)
        {
            Vertex v0 = vertices[triangle.v0];
            Vertex v1 = vertices[triangle.v1];
            Vertex v2 = vertices[triangle.v2];

            // vertices in front of the camera
            bool in0 = ((plane.normal * v0) + plane.Distance) > 0;
            bool in1 = ((plane.normal * v1) + plane.Distance) > 0;
            bool in2 = ((plane.normal * v2) + plane.Distance) > 0;

            int in_count = (in0 ? 1 : 0) + (in1 ? 1 : 0) + (in2 ? 1 : 0);

            if (in_count == 0)
            {
                //Console.WriteLine("count zero");
                // Nothing to do - the triangle is fully clipped out.
            }
            else if (in_count == 3)
            {
                // The triangle is fully in front of the plane.
                triangles.Add(triangle);
            }
            else if (in_count == 1)// one positive  
            {
                //Console.WriteLine("count one");
                // The triangle has one vertex in. Output is one clipped triangle.
            }
            else if (in_count == 2)// one negative
            {
                //Console.WriteLine("count two");
                // The triangle has two vertices in. Output is two clipped triangles.
            }

            return triangles;
        }

        private Model TransformAndClip(Plane[] clipping_planes, Model model, float scale, Mtx transform)
        {
            // Transform the bounding sphere, and attempt early discard.
            Vertex center = transform * model.bounds_center;
            float radius = model.bounds_radius * scale;
            for (int p = 0; p < clipping_planes.Length; p++)
            {
                float distance = (clipping_planes[p].normal * center) + clipping_planes[p].Distance;
                if (distance < -radius)
                {
                    return null;
                }
            }

            // Apply modelview transform.
            List<Vertex> vertices = new List<Vertex>();
            for (int i = 0; i < model.vertices.Length; i++)
            {
                vertices.Add(transform * model.vertices[i]);
            }

            // Clip the entire model against each successive plane.
            List<Triangle> triangles = new List<Triangle>(model.triangles);
            for (int p = 0; p < clipping_planes.Length; p++)
            {
                List<Triangle> new_triangles = new List<Triangle>();
                for (int i = 0; i < triangles.Count; i++)
                {
                    new_triangles = (ClipTriangle(triangles[i], clipping_planes[p], new_triangles, vertices));
                }
                triangles.AddRange(new_triangles);
            }

            return new Model(vertices.ToArray(), triangles.ToArray(), center, model.bounds_radius);
        }

        float SignedDistance(Plane plane, Vertex vertex)
        {
            Vertex normal = plane.normal;

            return (vertex.x * normal.x) + (vertex.y * normal.y) + (vertex.z * normal.z) + plane.Distance;
        }
        private Vertex ApplyTransform(Vertex v, Transform transform)
        {
            Vertex vertex;
            vertex = new Vertex(v.X, v.Y, v.Z);
            vertex *= transform.scale;
            vertex = transform.rotation;
            //Rotate(vertex, transform); 

            vertex += transform.traslation;
            return vertex;
        }
    }


    public class Transform
    {
        public float scale;
        public Vertex traslation;
        public Vertex rotation;

        public Transform(float scale, Vertex rotation, Vertex traslation)
        {
            this.scale = scale;
            this.rotation = rotation;
            this.traslation = traslation;
        }
    }
}
