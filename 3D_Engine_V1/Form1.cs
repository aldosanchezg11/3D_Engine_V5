using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace _3D_3ngine_V1
{
    public partial class Form1 : Form
    {

        public float z = 1f;
        
        public float rotateY = 0.0f;
        public float rotateX = 0.0f;
        public float rotateZ = 0.0f;

        public float translateX = 3.0f;
        public float translateY = -1.0f;
        public float translateZ = 20.0f;
        

        Vertex rotate1 = new Vertex(0,0,0);
        Vertex translate1 = new Vertex(2,0,10f);
        int scale1 = 1;

        Vertex rotate2 = new Vertex(0, 0, 0);
        Vertex translate2 = new Vertex(-2f, 0, 10f);
        int scale2 = 1;

        Vertex rotate3 = new Vertex(0, 0, 0);
        Vertex translate3 = new Vertex(6, 0, 10f);
        int scale3 = 1;

        public List<Vertex> rotationValues = new List<Vertex>();
        public List<Vertex> traslationValues = new List<Vertex>();
        public List<float> zValues = new List<float>();

        public TreeNode selectedNode;
        public int count = 0;
        public int playbackIndex = 0;

        public bool objSel = true;
        public bool recordinSet = false;

        string filename;
        string filename2;
        string filename3;

        public Form1()
        {
            InitializeComponent();
        }
        public void Init(string filename, string filename2, string filename3)
        {
            CreateNewOBJ(filename, filename2, filename3);
        }

        public void CreateNewOBJ(string filename, string filename2, string filename3)
        {
           ObjParser objectPar = new ObjParser(filename);
           ObjParser objectPar2 = new ObjParser(filename2);
           ObjParser objectPar3 = new ObjParser(filename3);

            //-----------------FIRST OBJECT--------------------------//
           Vertex[] vertices = objectPar.Vertices.Select(v => new Vertex(v[0], v[1], v[2])).ToArray();

           Triangle[] triangles = objectPar.Faces.Select(t => new Triangle(
                Array.IndexOf(vertices, vertices[t[0]]),
                Array.IndexOf(vertices, vertices[t[1]]),
                Array.IndexOf(vertices, vertices[t[2]]),
                Color.Black)).ToArray();

           Model cube = new Model(vertices, triangles, new Vertex(0, 0, 0), (float)Math.Sqrt(3));

            //-----------------SECOND OBJECT--------------------------//
           Vertex[] vertices2 = objectPar2.Vertices.Select(v => new Vertex(v[0], v[1], v[2])).ToArray();

           Triangle[] triangles2 = objectPar2.Faces.Select(t => new Triangle(
                Array.IndexOf(vertices, vertices[t[0]]),
                Array.IndexOf(vertices, vertices[t[1]]),
                Array.IndexOf(vertices, vertices[t[2]]),
                Color.Black)).ToArray();

           Model cube2 = new Model(vertices2, triangles2, new Vertex(0, 0, 0), (float)Math.Sqrt(3));

            //-----------------THIRD OBJECT--------------------------//
           Vertex[] vertices3 = objectPar3.Vertices.Select(v => new Vertex(v[0], v[1], v[2])).ToArray();

           Triangle[] triangles3 = objectPar3.Faces.Select(t => new Triangle(
                Array.IndexOf(vertices, vertices[t[0]]),
                Array.IndexOf(vertices, vertices[t[1]]),
                Array.IndexOf(vertices, vertices[t[2]]),
                Color.Black)).ToArray();

           Model cube3 = new Model(vertices3, triangles3, new Vertex(0, 0, 0), (float)Math.Sqrt(3));

           Instance[] instances = new Instance[3];
            
           instances[0] = new Instance(cube, translate1, Mtx.Rotate(rotate1), scale1);
           instances[1] = new Instance(cube2, translate2, Mtx.Rotate(rotate2), scale2);
           instances[2] = new Instance(cube3, translate3, Mtx.Rotate(rotate3), scale3);

           Rasterization raster = new Rasterization(PCT_CANVAS.Size, rotate1, translate1, trackBar4.Value, instances);


           PCT_CANVAS.Image = raster.Canvas;

        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag != null)
            {
                selectedNode = e.Node;
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

            if(e.Node.Text == Path.GetFileName(filename))
            {
                MessageBox.Show(Path.GetFileName(filename) + " selected");
                count = 0;


            } else if(e.Node.Text == Path.GetFileName(filename2))
            {
                MessageBox.Show(Path.GetFileName(filename2) + " selected");
                count = 1;


            } else if(e.Node.Text == Path.GetFileName(filename3))
            {
                MessageBox.Show(Path.GetFileName(filename3) + " selected");
                count = 2;
            }
        }

        public void Add_OBJ_Click(object sender, EventArgs e)
        { 

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "OBJ files (*.obj)|*.obj";
            dialog.Multiselect = true;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (dialog.FileNames.Length == 3)
                {
                    filename = dialog.FileNames[0];
                    filename2 = dialog.FileNames[1];
                    filename3 = dialog.FileNames[2];


                    //Add tree view
                    //TreeNode parentNode = treeView1.Nodes.Add("OBJ files");
                    TreeNode node1 = treeView1.Nodes.Add(Path.GetFileName(filename));
                    TreeNode node2 = treeView1.Nodes.Add(Path.GetFileName(filename2));
                    TreeNode node3 = treeView1.Nodes.Add(Path.GetFileName(filename3));


                    canvasTimer.Enabled = true;
                    canvasTimer.Start();

                    Init(filename, filename2, filename3);
                    // Do something with objectPar and objectPar2
                }
                else
                {
                    MessageBox.Show("Please select three OBJ files.");
                }
            }

            
        }

        public void trackBar1_Scroll(object sender, EventArgs e)
        {
            YrotLabel.Text = "Y:" + trackBar1.Value.ToString();
        }

        public void trackBar2_Scroll(object sender, EventArgs e)
        {
            XrotLabel.Text = "X:" + trackBar2.Value.ToString();
        }

        public void trackBar3_Scroll(object sender, EventArgs e)
        {
            ZrotLabel.Text = "Z:" + trackBar3.Value.ToString();
        }

        public void trackBar4_Scroll(object sender, EventArgs e)
        {
            scaleLabel.Text = "Scale:" + trackBar4.Value.ToString();
        }

        public void trackBarTrY_Scroll(object sender, EventArgs e)
        {
            YTrLabel.Text ="Y:" + trackBarTrY.Value.ToString();
        }

        public void trackBarTrX_Scroll(object sender, EventArgs e)
        {
            TrXlabel.Text = "X:" + trackBarTrX.Value.ToString();
        }

        public void trackBarTrZ_Scroll(object sender, EventArgs e)
        {
            TrZlabel.Text = "Z: " + trackBarTrZ.Value.ToString();
        }

        public void canvasTimer_Tick(object sender, EventArgs e)
        {
            Init(filename, filename2, filename3);
            moveOBJ(count);
            PCT_CANVAS.Invalidate();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            canvasTimer.Stop();
        }

        private void Stop_BTN_Click(object sender, EventArgs e)
        {
            rotationValues.Clear();
            traslationValues.Clear();
            zValues.Clear();
            recordinSet = false;
        }

        public void Record_BTN_Click(object sender, EventArgs e)
        {
            Record_Timer.Interval = 50; //100
            Record_Timer.Start();
            Record_Timer.Enabled = true;
            Record_BTN.Enabled = false;


            timer1.Start();
            timer1.Enabled = true;
            timer1.Interval = 10000; 
        }

        private void Record_Timer_Tick(object sender, EventArgs e)
        {
            switch (count)
            {
                case 0:
                    rotate1 = new Vertex(trackBar2.Value, trackBar1.Value, trackBar3.Value);
                    translate1 = new Vertex(trackBarTrX.Value, trackBarTrY.Value, trackBarTrZ.Value);
                    z = trackBar4.Value;

                    rotationValues.Add(rotate1);
                    traslationValues.Add(translate1);
                    zValues.Add(z);

                    recordinSet = true;
                    break;
                case 1:
                    rotate2 = new Vertex(trackBar2.Value, trackBar1.Value, trackBar3.Value);
                    translate2 = new Vertex(trackBarTrX.Value, trackBarTrY.Value, trackBarTrZ.Value);
                    z = trackBar4.Value;

                    rotationValues.Add(rotate2);
                    traslationValues.Add(translate2);
                    zValues.Add(z);
                    recordinSet = true;
                    break;
                case 2:
                    rotate3 = new Vertex(trackBar2.Value, trackBar1.Value, trackBar3.Value);
                    translate3 = new Vertex(trackBarTrX.Value, trackBarTrY.Value, trackBarTrZ.Value);
                    z = trackBar4.Value;

                    rotationValues.Add(rotate3);
                    traslationValues.Add(translate3);
                    zValues.Add(z);
                    recordinSet = true;
                    break;

            }

            
        }

        public void Play_BTN_Click(object sender, EventArgs e)
        {
            if (!recordinSet)
            {
                MessageBox.Show("No recording available");
            }
            else
            {
                Play_Timer.Interval = 50; //125
                Play_Timer.Start();
            }
        }

        public void Play_Timer_Tick(object sender, EventArgs e)
        {
            if(playbackIndex < rotationValues.Count)
            {
                trackBar2.Value = (int)rotationValues[playbackIndex].X;
                trackBar1.Value = (int)rotationValues[playbackIndex].Y;
                trackBar3.Value = (int)rotationValues[playbackIndex].Z;

                trackBarTrX.Value = (int)traslationValues[playbackIndex].X;
                trackBarTrY.Value = (int)traslationValues[playbackIndex].Y;
                trackBarTrZ.Value = (int)traslationValues[playbackIndex].Z;

                trackBar4.Value = (int)zValues[playbackIndex];

                moveOBJ(count);

                playbackIndex++;
            }
            else
            {
                playbackIndex = 0;
                Play_Timer.Stop();
                Play_Timer.Enabled = false;
                Record_BTN.Enabled = true;
                
                MessageBox.Show("End of recording");
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //This runs 5 seconds after the Record Button is clicked
            Record_Timer.Stop();
            Record_Timer.Enabled = false;
            Record_BTN.Enabled = true;

            timer1.Stop();
            timer1.Enabled = false;
            MessageBox.Show("Screen has stopped recording");

        }

        public void moveOBJ(int count)
        {
            switch (count)
            {
                case 0:

                    rotate1 = new Vertex(trackBar2.Value, trackBar1.Value, trackBar3.Value);
                    translate1 = new Vertex(trackBarTrX.Value, trackBarTrY.Value, trackBarTrZ.Value);
                    scale1 = trackBar4.Value;
                    break;

                case 1:

                    rotate2 = new Vertex(trackBar2.Value, trackBar1.Value, trackBar3.Value);
                    translate2 = new Vertex(trackBarTrX.Value, trackBarTrY.Value, trackBarTrZ.Value);
                    scale2 = trackBar4.Value;
                    break;

                case 2:

                    rotate3 = new Vertex(trackBar2.Value, trackBar1.Value, trackBar3.Value);
                    translate3 = new Vertex(trackBarTrX.Value, trackBarTrY.Value, trackBarTrZ.Value);
                    scale3 = trackBar4.Value;
                    break;
            }
        }

    }
}
