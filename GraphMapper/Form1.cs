﻿using Graph;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace GraphMapper
{
    public partial class GraphMapperForm : Form
    {
        private Pen wayPen = new Pen(Brushes.Brown, 6.0f);
        private Pen roomPen = Pens.Red;
        private Pen interPen = Pens.Blue;
        private RoomDialog roomDialoge = new RoomDialog();
        private FloorNumberDialog floorDialoge = new FloorNumberDialog();
        private int roomId;

        private Image drawArea;
        private Image lastArea;
        private GraphNode focusedNode;
        private GraphNode lastFocusedNode;
        private List<GraphNode> nodes = new List<GraphNode>();
        private bool stairsMode;

        private const int NodeRadius = 20;
        private const int CursorSpeed = 5;

        public GraphMapperForm()
        {
            wayPen.SetLineCap(System.Drawing.Drawing2D.LineCap.Round, System.Drawing.Drawing2D.LineCap.Round, System.Drawing.Drawing2D.DashCap.Round);
            InitializeComponent();
            this.KeyDown += (sender, e) =>
            {
                switch (e.KeyCode)
                {
                    case Keys.Left:
                        Cursor.Position = new System.Drawing.Point(Cursor.Position.X - CursorSpeed, Cursor.Position.Y);
                        break;
                    case Keys.Right:
                        Cursor.Position = new System.Drawing.Point(Cursor.Position.X + CursorSpeed, Cursor.Position.Y);
                        break;
                    case Keys.Up:
                        Cursor.Position = new System.Drawing.Point(Cursor.Position.X, Cursor.Position.Y - CursorSpeed);
                        break;
                    case Keys.Down:
                        Cursor.Position = new System.Drawing.Point(Cursor.Position.X, Cursor.Position.Y + CursorSpeed);
                        break;
                }
            };
            this.KeyPress += (sender, e) =>
            {
                if (e.KeyChar == 'z')
                {
                    if (nodes.Count > 2 && lastArea != null)
                    {
                        lastFocusedNode.Neighbours.Remove(focusedNode);
                        focusedNode.Neighbours.Remove(lastFocusedNode);
                        nodes.Remove(focusedNode);
                        focusedNode = lastFocusedNode;
                        drawArea.Dispose();
                        drawArea = (Image)lastArea.Clone();
                        lastArea = null;
                        pictureBox1.Image = drawArea;
                    }
                }
                else if (e.KeyChar == 'l')
                {
                    stairsMode = !stairsMode;
                    var activeStatus = stairsMode ? "активирован" : "деактивирован";
                    MessageBox.Show($"Режим лестниц {activeStatus}!", "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (e.KeyChar == '[')
                {
                    pictureBox1_MouseClick(null, new MouseEventArgs(MouseButtons.Left, 1, Cursor.Position.X - 9 - this.AutoScrollPosition.X, Cursor.Position.Y - 47 - this.AutoScrollPosition.Y, 0));

                }
                else if (e.KeyChar == ']')
                {
                    pictureBox1_MouseClick(null, new MouseEventArgs(MouseButtons.Right, 1, Cursor.Position.X - 9 - this.AutoScrollPosition.X, Cursor.Position.Y - 47 - this.AutoScrollPosition.Y, 0));
                }
            };
        }

        private void openToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    pictureBox1.Load(openFileDialog1.FileName);
                    drawArea = pictureBox1.Image;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Load image error: " + ex.Message);
                }
            }
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (drawArea != null)
            {
                var x = e.X;
                var y = e.Y;
                var newFocusFound = false;
                foreach (var node in nodes)
                {
                    var nodeX = node.Point.X;
                    var nodeY = node.Point.Y;
                    var length = Math.Sqrt((x - nodeX) * (x - nodeX) + (y - nodeY) * (y - nodeY));
                    if (length < NodeRadius)
                    {
                        if (stairsMode)
                        {
                            if (node.IsIntermediate)
                            {
                                if (MessageBox.Show($"Вы действительно хотите сделать этот узел лестницей?\nID узла: {node.Id}", "Внимание!", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                                {
                                    node.IsStairs = true;
                                }
                            }
                            else
                            {
                                MessageBox.Show("Выбраный узел является комнатой и не может быть лестницей.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            return;
                        }
                        else
                        {
                            focusedNode = node;
                            newFocusFound = true;
                        }
                    }
                }
                if (newFocusFound == false)
                {
                    using (var gr = Graphics.FromImage(drawArea))
                    {
                        Pen pen = null;
                        var roomName = "*Unknown*";
                        var isIntermediate = false;
                        if (e.Button == MouseButtons.Left)
                        {
                            pen = roomPen;
                            if (roomDialoge.ShowDialog() != DialogResult.OK)
                            {
                                return;
                            }
                            roomName = roomDialoge.RoomName;
                        }
                        else if (e.Button == MouseButtons.Right)
                        {
                            pen = interPen;
                            isIntermediate = true;
                        }
                        var newNode = new GraphNode()
                        {
                            Id = roomId++,
                            Point = new Graph.Point(x, y),
                            IsIntermediate = isIntermediate,
                            RoomName = roomName,
                        };

                        gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        lastArea?.Dispose();
                        lastArea = (Image)pictureBox1.Image.Clone();

                        gr.DrawEllipse(pen, x - NodeRadius, y - NodeRadius, NodeRadius * 2, NodeRadius * 2);
                        nodes.Add(newNode);
                        if (focusedNode == null)
                        {
                            focusedNode = newNode;
                        }
                        var focusedX = focusedNode.Point.X;
                        var focusedY = focusedNode.Point.Y;
                        newNode.Neighbours.Add(focusedNode);
                        focusedNode.Neighbours.Add(newNode);
                        gr.DrawLine(wayPen, x, y, focusedX, focusedY);
                        lastFocusedNode = focusedNode;
                        focusedNode = newNode;

                        pictureBox1.Image = drawArea;
                    }
                }
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (floorDialoge.ShowDialog() == DialogResult.OK)
            {
                SetFloorNumber(floorDialoge.FloorNumber, floorDialoge.FloorPartNumber);

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var stream = File.Open(saveFileDialog1.FileName, FileMode.Create))
                        {
                            var graphService = new GraphService(null);
                            graphService.Save(stream, focusedNode);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Save graph error: " + ex.Message);
                    }
                }
            }
        }

        private void SetFloorNumber(int floorNumber, int floorPartNumber)
        {
            foreach (var node in nodes)
            {
                node.FloorNumber = floorNumber;
                node.FloorPartNumber = floorPartNumber;
            }
        }
    }
}
