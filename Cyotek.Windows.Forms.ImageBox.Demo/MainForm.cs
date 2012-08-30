﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Cyotek.Windows.Forms.Demo
{
  // Cyotek ImageBox
  // Copyright (c) 2010-2012 Cyotek. All Rights Reserved.
  // http://cyotek.com

  // If you use this control in your applications, attribution or donations are welcome.

  // ImageBox sample project
  // http://cyotek.com/article/display/creating-a-scrollable-and-zoomable-image-viewer-in-csharp-part-1
  // http://cyotek.com/article/display/creating-a-scrollable-and-zoomable-image-viewer-in-csharp-part-2
  // http://cyotek.com/article/display/creating-a-scrollable-and-zoomable-image-viewer-in-csharp-part-3
  // http://cyotek.com/article/display/creating-a-scrollable-and-zoomable-image-viewer-in-csharp-part-4
  // http://cyotek.com/article/display/creating-an-image-viewer-in-csharp-part-5-selecting-part-of-an-image

  // Preview image based on Glyfz sampler - http://www.glyfz.com/sampler.htm
  // Large preview image from http://www.crazythemes.com/colorful-abstract-widescreen-wallpapers-vol2/2153
  // Toolbar icons from Fugue Icons - http://p.yusukekamiyamane.com/

  public partial class MainForm : Form
  {
    private Image _previewImage;

    public MainForm()
    {
      InitializeComponent();
    }

    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);

      this.FillZoomLevels();
      this.OpenImage(Properties.Resources.Sample);

      imageBox.SelectionMode = ImageBoxSelectionMode.Zoom;
      imageBox.AllowClickZoom = true;
    }

    private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
    {
      using (Form dialog = new AboutDialog())
        dialog.ShowDialog(this);
    }

    private void actualSizeToolStripButton_Click(object sender, EventArgs e)
    {
      imageBox.ActualSize();
    }

    private void copyToolStripMenuItem_Click(object sender, EventArgs e)
    {
      try
      {
        Clipboard.Clear();
        Clipboard.SetImage(imageBox.GetSelectedImage() ?? imageBox.Image);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void DrawBox(Graphics graphics, Color color, RectangleF rectangle, double scale)
    {
      float penWidth;

      penWidth = 2 * (float)scale;

      using (SolidBrush brush = new SolidBrush(Color.FromArgb(64, color)))
        graphics.FillRectangle(brush, rectangle);

      using (Pen pen = new Pen(color, penWidth) { DashStyle = DashStyle.Dot, DashCap = DashCap.Round })
        graphics.DrawRectangle(pen, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
    }

    private void exitToolStripMenuItem_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void FillZoomLevels()
    {
      zoomLevelsToolStripComboBox.Items.Clear();

      foreach (int zoom in imageBox.ZoomLevels)
        zoomLevelsToolStripComboBox.Items.Add(string.Format("{0}%", zoom));
    }

    private string FormatPoint(Point point)
    {
      return string.Format("X:{0}, Y:{1}", point.X, point.Y);
    }

    private string FormatRectangle(RectangleF rect)
    {
      return string.Format("X:{0}, Y:{1}, W:{2}, H:{3}", (int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
    }

    private void imageBox_MouseLeave(object sender, EventArgs e)
    {
      cursorToolStripStatusLabel.Text = string.Empty;
    }

    private void imageBox_MouseMove(object sender, MouseEventArgs e)
    {
      this.UpdateCursorPosition(e.Location);
    }

    private void imageBox_Paint(object sender, PaintEventArgs e)
    {
      // highlight the image
      if (showImageRegionToolStripButton.Checked)
        this.DrawBox(e.Graphics, Color.CornflowerBlue, imageBox.GetImageViewPort(), 1);

      // show the region that will be drawn from the source image
      if (showSourceImageRegionToolStripButton.Checked)
        this.DrawBox(e.Graphics, Color.Firebrick, new RectangleF(imageBox.GetImageViewPort().Location, imageBox.GetSourceImageRegion().Size), 1);
    }

    private void imageBox_Resize(object sender, EventArgs e)
    {
      this.UpdateStatusBar();
    }

    private void imageBox_Scroll(object sender, ScrollEventArgs e)
    {
      this.UpdateStatusBar();
    }

    private void imageBox_Selected(object sender, EventArgs e)
    {
      this.UpdatePreviewImage();
    }

    private void imageBox_SelectionRegionChanged(object sender, EventArgs e)
    {
      selectionToolStripStatusLabel.Text = this.FormatRectangle(imageBox.SelectionRegion);
    }

    private void imageBox_ZoomChanged(object sender, EventArgs e)
    {
      this.UpdateStatusBar();
    }

    private void imageBox_ZoomLevelsChanged(object sender, EventArgs e)
    {
      this.FillZoomLevels();
    }

    private void OpenImage(Image image)
    {
      imageBox.Image = image;
      imageBox.ZoomToFit();

      this.UpdateStatusBar();
      this.UpdatePreviewImage();
    }

    private void openToolStripMenuItem_Click(object sender, EventArgs e)
    {
      using (FileDialog dialog = new OpenFileDialog())
      {
        dialog.Filter = "All Supported Images (*.bmp;*.dib;*.rle;*.gif;*.jpg;*.png)|*.bmp;*.dib;*.rle;*.gif;*.jpg;*.png|Bitmaps (*.bmp;*.dib;*.rle)|*.bmp;*.dib;*.rle|Graphics Interchange Format (*.gif)|*.gif|Joint Photographic Experts (*.jpg)|*.jpg|Portable Network Graphics (*.png)|*.png|All Files (*.*)|*.*";
        dialog.DefaultExt = "png";

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
          try
          {
            this.OpenImage(Image.FromFile(dialog.FileName));
          }
          catch (Exception ex)
          {
            MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
          }
        }
      }
    }

    private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
    {
      imageBox.SelectAll();
    }

    private void selectNoneToolStripMenuItem_Click(object sender, EventArgs e)
    {
      imageBox.SelectNone();
    }

    private void showImageRegionToolStripButton_Click(object sender, EventArgs e)
    {
      imageBox.Invalidate();
    }

    private void UpdateCursorPosition(Point location)
    {
      Point point;

      point = imageBox.PointToImage(location);

      if (point != Point.Empty)
        cursorToolStripStatusLabel.Text = this.FormatPoint(point);
      else
        cursorToolStripStatusLabel.Text = string.Empty;
    }

    private void UpdatePreviewImage()
    {
      if (_previewImage != null)
        _previewImage.Dispose();

      _previewImage = imageBox.GetSelectedImage();

      previewImageBox.Image = _previewImage;
    }

    private void UpdateStatusBar()
    {
      zoomLevelsToolStripComboBox.Text = string.Format("{0}%", imageBox.Zoom);
      autoScrollPositionToolStripStatusLabel.Text = this.FormatPoint(imageBox.AutoScrollPosition);
      imageSizeToolStripStatusLabel.Text = this.FormatRectangle(imageBox.GetImageViewPort());
      zoomToolStripStatusLabel.Text = string.Format("{0}%", imageBox.Zoom);
    }

    private void zoomInToolStripButton_Click(object sender, EventArgs e)
    {
      imageBox.ZoomIn();
    }

    private void zoomLevelsToolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      int zoom;

      zoom = Convert.ToInt32(zoomLevelsToolStripComboBox.Text.Substring(0, zoomLevelsToolStripComboBox.Text.Length - 1));

      imageBox.Zoom = zoom;
    }

    private void zoomOutToolStripButton_Click(object sender, EventArgs e)
    {
      imageBox.ZoomOut();
    }
  }
}