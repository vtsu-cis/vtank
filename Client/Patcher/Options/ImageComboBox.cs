using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace VTankOptionsSpace
{
    /// <summary>
    /// Class responsible for drawing images inside of a combo box.
    /// Courtesy of: http://www.csharphelp.com/2006/08/combobox-with-images/
    /// </summary>
    class ImageComboBox : ComboBox
    {
        private ImageList imageList;
        public ImageList ImageList
        {
            get { return imageList; }
            set { imageList = value; }
        }

        public ImageComboBox()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
        }

        protected override void OnDrawItem(DrawItemEventArgs ea)
        {
            ea.DrawBackground();
            ea.DrawFocusRectangle();

            ImageComboBoxItem item;
            Size imageSize = imageList.ImageSize;
            Rectangle bounds = ea.Bounds;

            try
            {
                item = (ImageComboBoxItem)Items[ea.Index];

                if (item.ImageIndex != -1)
                {
                    imageList.Draw(ea.Graphics, bounds.Left, bounds.Top,
                    item.ImageIndex);
                    ea.Graphics.DrawString(item.Text, ea.Font, new
                    SolidBrush(ea.ForeColor), bounds.Left + imageSize.Width, bounds.Top);
                }
                else
                {
                    ea.Graphics.DrawString(item.Text, ea.Font, new
                    SolidBrush(ea.ForeColor), bounds.Left, bounds.Top);
                }
            }
            catch
            {
                if (ea.Index != -1)
                {
                    ea.Graphics.DrawString(Items[ea.Index].ToString(), ea.Font, new
                    SolidBrush(ea.ForeColor), bounds.Left, bounds.Top);
                }
                else
                {
                    ea.Graphics.DrawString(Text, ea.Font, new
                    SolidBrush(ea.ForeColor), bounds.Left, bounds.Top);
                }
            }

            base.OnDrawItem(ea);
        }
    }

    class ImageComboBoxItem
    {
        private string _text;
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        private int _imageIndex;
        public int ImageIndex
        {
            get { return _imageIndex; }
            set { _imageIndex = value; }
        }

        public ImageComboBoxItem()
            : this("")
        {
        }

        public ImageComboBoxItem(string text)
            : this(text, -1)
        {
        }

        public ImageComboBoxItem(string text, int imageIndex)
        {
            _text = text;
            _imageIndex = imageIndex;
        }

        public override string ToString()
        {
            return _text;
        }
    }
}
