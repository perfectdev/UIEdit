using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using UIEdit.Models;
using UIEdit.Utils;

namespace UIEdit.Controllers {
    public class LayoutController {
        public string SourceText { get; set; }
        public UIDialog Dialog { get; set; }

        public Exception Parse(string text, string path) {
            var isValid = true;
            try {
                var tmpFileName = string.Format(@"{0}\tmp.xml", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
                File.WriteAllText(tmpFileName, text);
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(tmpFileName);
                Dialog = new UIDialog {
                                          Name = xmlDoc.DocumentElement.Attributes["Name"].Value
                                      };
                if (xmlDoc.DocumentElement.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Width"))
                    Dialog.Width = Convert.ToDouble(xmlDoc.DocumentElement.Attributes["Width"].Value);
                if (xmlDoc.DocumentElement.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Height"))
                    Dialog.Height = Convert.ToDouble(xmlDoc.DocumentElement.Attributes["Height"].Value);
                if (xmlDoc.DocumentElement.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "Resource")) {
                    var dlgRes = xmlDoc.DocumentElement.ChildNodes.Cast<XmlNode>().FirstOrDefault(t => t.Name == "Resource");
                    if (dlgRes.ChildNodes.Cast<XmlNode>().Any(t=>t.Name == "FrameImage")) 
                        Dialog.FrameImage = new UIResourceFrameImage {
                            FileName = string.Format(@"{0}\{1}", path, dlgRes.ChildNodes.Cast<XmlNode>().FirstOrDefault(t => t.Name == "FrameImage").Attributes["FileName"].Value)
                        };
                }

                foreach (XmlNode element in xmlDoc.DocumentElement.ChildNodes) {
                    switch (element.Name) {
                        case "IMAGEPICTURE":
                            var imageControl = new UIImagePicture {
                                Name = element.Attributes["Name"].Value,
                                FileName = element.FirstChild != null && element.FirstChild.FirstChild != null
                                    ? string.Format(@"{0}\{1}", path, element.FirstChild.FirstChild.Attributes["FileName"].Value)
                                    : ""
                            };
                            if (element.Attributes.Cast<XmlAttribute>().Any(t=>t.Name == "x")) imageControl.X = Convert.ToDouble(element.Attributes["x"].Value);
                            if (element.Attributes.Cast<XmlAttribute>().Any(t=>t.Name == "y")) imageControl.Y = Convert.ToDouble(element.Attributes["y"].Value);
                            if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Width")) imageControl.Width = Convert.ToDouble(element.Attributes["Width"].Value);
                            if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Height")) imageControl.Height = Convert.ToDouble(element.Attributes["Height"].Value);
                            Dialog.ImagePictures.Add(imageControl);
                            break;
                        case "SCROLL":
                            var scrollControl = new UIScroll {
                                Name = element.Attributes["Name"].Value
                            };
                            if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "x")) scrollControl.X = Convert.ToDouble(element.Attributes["x"].Value);
                            if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "y")) scrollControl.Y = Convert.ToDouble(element.Attributes["y"].Value);
                            if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Width")) scrollControl.Width = Convert.ToDouble(element.Attributes["Width"].Value);
                            if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Height")) scrollControl.Height = Convert.ToDouble(element.Attributes["Height"].Value);
                            foreach (var node in element.ChildNodes.Cast<XmlNode>().Where(node => node.Name == "Resource")) {
                                var upImage = node.ChildNodes.Cast<XmlNode>().FirstOrDefault(t => t.Name == "UpImage");
                                if (upImage != null) scrollControl.UpImage = string.Format(@"{0}\{1}", path, upImage.Attributes["FileName"].Value);

                                var downImage = node.ChildNodes.Cast<XmlNode>().FirstOrDefault(t => t.Name == "DownImage");
                                if (downImage != null) scrollControl.DownImage = string.Format(@"{0}\{1}", path, downImage.Attributes["FileName"].Value);

                                var scrollImage = node.ChildNodes.Cast<XmlNode>().FirstOrDefault(t => t.Name == "ScrollImage");
                                if (scrollImage != null) scrollControl.ScrollImage = string.Format(@"{0}\{1}", path, scrollImage.Attributes["FileName"].Value);

                                var barFrameImage = node.ChildNodes.Cast<XmlNode>().FirstOrDefault(t => t.Name == "BarFrameImage");
                                if (barFrameImage != null) scrollControl.BarFrameImage = string.Format(@"{0}\{1}", path, barFrameImage.Attributes["FileName"].Value);
                            }
                            Dialog.Scrolls.Add(scrollControl);
                            break;
                        case "RADIO":
                            var radioControl = new UIRadioButton {
                                Name = element.Attributes["Name"].Value,
                            };
                            if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "x")) radioControl.X = Convert.ToDouble(element.Attributes["x"].Value);
                            if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "y")) radioControl.Y = Convert.ToDouble(element.Attributes["y"].Value);
                            if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Width")) radioControl.Width = Convert.ToDouble(element.Attributes["Width"].Value);
                            if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Height")) radioControl.Height = Convert.ToDouble(element.Attributes["Height"].Value);
                            if (element.ChildNodes != null && element.ChildNodes[0].Name == "Hint") {
                                radioControl.Hint = element.ChildNodes != null
                                                   ? element.ChildNodes[0].Attributes["String"].Value
                                                   : "";
                            }
                            foreach (var node in element.ChildNodes.Cast<XmlNode>().Where(node => node.Name == "Resource")) {
                                var normalImage = node.ChildNodes.Cast<XmlNode>().FirstOrDefault(t => t.Name == "NormalImage");
                                if (normalImage != null) radioControl.NormalImage = string.Format(@"{0}\{1}", path, normalImage.Attributes["FileName"].Value);

                                var checkedImage = node.ChildNodes.Cast<XmlNode>().FirstOrDefault(t => t.Name == "CheckedImage");
                                if (checkedImage != null) radioControl.CheckedImage = string.Format(@"{0}\{1}", path, checkedImage.Attributes["FileName"].Value);

                            }
                            Dialog.RadioButtons.Add(radioControl);
                            break;
                        case "STILLIMAGEBUTTON":
                            var buttonControl = new UIStillImageButton {
                                Name = element.Attributes["Name"].Value,
                            };
                            if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "x")) buttonControl.X = Convert.ToDouble(element.Attributes["x"].Value);
                            if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "y")) buttonControl.Y = Convert.ToDouble(element.Attributes["y"].Value);
                            if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Width")) buttonControl.Width = Convert.ToDouble(element.Attributes["Width"].Value);
                            if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Height")) buttonControl.Height = Convert.ToDouble(element.Attributes["Height"].Value);
                            if (element.ChildNodes != null && element.ChildNodes[0].Name == "Hint") {
                                buttonControl.Hint = element.ChildNodes != null
                                                   ? element.ChildNodes[0].Attributes["String"].Value
                                                   : "";
                            }
                            if (element.ChildNodes.Cast<XmlNode>().Any(node => node.Name == "Text")) {
                                var textNode = element.ChildNodes.Cast<XmlNode>().First(node => node.Name == "Text");
                                if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "String")) buttonControl.Text = textNode.Attributes["String"].Value;
                                if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FontSize")) buttonControl.FontSize = Convert.ToDouble(textNode.Attributes["FontSize"].Value);
                                if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Color")) buttonControl.Color = textNode.Attributes["Color"].Value;
                                if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "OutlineColor")) buttonControl.OutlineColor = textNode.Attributes["OutlineColor"].Value;
                                if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "InnerColor")) buttonControl.InnerColor = textNode.Attributes["InnerColor"].Value;
                            }
                            foreach (var node in element.ChildNodes.Cast<XmlNode>().Where(node => node.Name == "Resource")) {
                                var upImage = node.ChildNodes.Cast<XmlNode>().FirstOrDefault(t => t.Name == "FrameUpImage");
                                if (upImage != null) buttonControl.UpImage = string.Format(@"{0}\{1}", path, upImage.Attributes["FileName"].Value);

                                var downImage = node.ChildNodes.Cast<XmlNode>().FirstOrDefault(t => t.Name == "FrameDownImage");
                                if (downImage != null) buttonControl.DownImage = string.Format(@"{0}\{1}", path, downImage.Attributes["FileName"].Value);

                                var hoverImage = node.ChildNodes.Cast<XmlNode>().FirstOrDefault(t => t.Name == "FrameOnHoverImage");
                                if (hoverImage != null) buttonControl.HoverImage = string.Format(@"{0}\{1}", path, hoverImage.Attributes["FileName"].Value);

                            }
                            Dialog.StillImageButtons.Add(buttonControl);
                            break;
                        case "LABEL":
                            var labelControl = new UILabel {
                                Name = element.Attributes["Name"].Value,
                            };
                            if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "x")) labelControl.X = Convert.ToDouble(element.Attributes["x"].Value);
                            if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "y")) labelControl.Y = Convert.ToDouble(element.Attributes["y"].Value);
                            if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Width")) labelControl.Width = Convert.ToDouble(element.Attributes["Width"].Value);
                            if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Height")) labelControl.Height = Convert.ToDouble(element.Attributes["Height"].Value);
                            if (element.ChildNodes.Cast<XmlNode>().Any(node => node.Name == "Text")) {
                                var textNode = element.ChildNodes.Cast<XmlNode>().First(node => node.Name == "Text");
                                if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "String")) labelControl.Text = textNode.Attributes["String"].Value;
                                if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FontSize")) labelControl.FontSize = Convert.ToDouble(textNode.Attributes["FontSize"].Value);
                                if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Color")) labelControl.Color = textNode.Attributes["Color"].Value;
                                if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "OutlineColor")) labelControl.OutlineColor = textNode.Attributes["OutlineColor"].Value;
                                if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "TextUpperColor")) labelControl.TextUpperColor = textNode.Attributes["TextUpperColor"].Value;
                                if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "TextLowerColor")) labelControl.TextLowerColor = textNode.Attributes["TextLowerColor"].Value;
                            }
                            Dialog.Labels.Add(labelControl);
                            break;
                        case "EDIT":
                            var editControl = new UIEditBox {
                                Name = element.Attributes["Name"].Value
                            };
                            if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "x")) editControl.X = Convert.ToDouble(element.Attributes["x"].Value);
                            if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "y")) editControl.Y = Convert.ToDouble(element.Attributes["y"].Value);
                            if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Width")) editControl.Width = Convert.ToDouble(element.Attributes["Width"].Value);
                            if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Height")) editControl.Height = Convert.ToDouble(element.Attributes["Height"].Value);
                            if (element.ChildNodes.Cast<XmlNode>().Any(node => node.Name == "Text")) {
                                var textNode = element.ChildNodes.Cast<XmlNode>().First(node => node.Name == "Text");
                                if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "String")) editControl.Text = textNode.Attributes["String"].Value;
                                if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FontSize")) editControl.FontSize = Convert.ToDouble(textNode.Attributes["FontSize"].Value);
                                if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Color")) editControl.Color = textNode.Attributes["Color"].Value;
                            }
                            foreach (var node in element.ChildNodes.Cast<XmlNode>().Where(node => node.Name == "Resource")) {
                                var frameImage = node.ChildNodes.Cast<XmlNode>().FirstOrDefault(t => t.Name == "FrameImage");
                                if (frameImage != null) editControl.FrameImage = new UIResourceFrameImage { FileName = string.Format(@"{0}\{1}", path, frameImage.Attributes["FileName"].Value) };
                            }
                            Dialog.Edits.Add(editControl);
                            break;
                    }
                }

                File.Delete(tmpFileName);
            }
            catch (Exception e) {
                return e;
            }

            return null;
        }

        public void RefreshLayout(Canvas dialogCanvas) {
            dialogCanvas.Children.Clear();
            dialogCanvas.Children.Add(new Image {
                ToolTip = Dialog.Name,
                Width = Dialog.Width,
                Height = Dialog.Height,
                Stretch = Stretch.Fill,
                StretchDirection = StretchDirection.Both,
                Source = Dialog.FrameImage == null ? new BitmapImage() : Core.TrueStretchImage(Dialog.FrameImage.FileName, Dialog.Width, Dialog.Height)
            });
            foreach (var control in Dialog.Edits) {
                dialogCanvas.Children.Add(new Image {
                    ToolTip = control.Name,
                    Width = control.Width,
                    Height = control.Height,
                    Stretch = Stretch.Fill,
                    StretchDirection = StretchDirection.Both,
                    Margin = new Thickness(control.X, control.Y, 0, 0),
                    Source = Core.TrueStretchImage(control.FrameImage != null ? control.FrameImage.FileName : null, control.Width, control.Height)
                });
                var tb = new TextBlock {
                    ToolTip = control.Name,
                    Text = string.IsNullOrEmpty(control.Text) ? control.Name : control.Text,
                    Width = control.Width,
                    Height = control.Height,
                    Margin = new Thickness(control.X, control.Y + ((control.Height - control.FontSize) / 4), 0, 0),
                    Foreground = Core.GetColorBrushFromString(control.Color)
                };
                if (control.FontSize > 1) tb.FontSize = control.FontSize;
                dialogCanvas.Children.Add(tb);
            }
            foreach (var pic in Dialog.ImagePictures) {
                dialogCanvas.Children.Add(new Image {
                    ToolTip = pic.Name,
                    Width = pic.Width,
                    Height = pic.Height,
                    Margin = new Thickness(pic.X, pic.Y, 0, 0),
                    Source = Core.GetImageSourceFromFileName(pic.FileName)
                });
            }
            foreach (var control in Dialog.Scrolls) {
                var scrollControl = new Canvas {
                    ToolTip = control.Name,
                    Width = control.Width,
                    Height = control.Height,
                    Margin = new Thickness(control.X, control.Y, 0, 0)
                };
                scrollControl.Children.Add(new Image {
                    ToolTip = control.Name,
                    Source = Core.GetImageSourceFromFileName(control.BarFrameImage)
                });
                dialogCanvas.Children.Add(scrollControl);
            }
            foreach (var control in Dialog.RadioButtons) {
                dialogCanvas.Children.Add(new Image {
                    ToolTip = string.IsNullOrEmpty(control.Hint) ? control.Name : string.Format("{0}: {1}", control.Name, control.Hint),
                    Width = control.Width,
                    Height = control.Height,
                    Stretch = Stretch.Fill,
                    StretchDirection = StretchDirection.Both,
                    Margin = new Thickness(control.X, control.Y, 0, 0),
                    Source = Core.GetImageSourceFromFileName(control.NormalImage)
                });
            }
            foreach (var control in Dialog.StillImageButtons) {
                dialogCanvas.Children.Add(new Image {
                    ToolTip = string.IsNullOrEmpty(control.Hint) ? control.Name : control.Hint,
                    Width = control.Width,
                    Height = control.Height,
                    Stretch = Stretch.Fill,
                    StretchDirection = StretchDirection.Both,
                    Margin = new Thickness(control.X, control.Y, 0, 0),
                    Source = Core.TrueStretchImage(control.UpImage, control.Width, control.Height)
                });
                var tb = new TextBlock {
                    ToolTip = string.IsNullOrEmpty(control.Hint) ? control.Name : string.Format("{0}: {1}", control.Name, control.Hint),
                    Text = string.IsNullOrEmpty(control.Text) ? control.Name : control.Text,
                    Width = control.Width,
                    Height = control.Height,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(control.X, control.Y + ((control.Height - control.FontSize) / 4), 0, 0),
                    Foreground = Core.GetColorBrushFromString(control.Color),
                    FontWeight = FontWeight.FromOpenTypeWeight(999)
                };
                if (control.FontSize > 1) tb.FontSize = control.FontSize;
                dialogCanvas.Children.Add(tb);
            }
            foreach (var control in Dialog.Labels) {
                var tb = new TextBlock {
                    Text = string.IsNullOrEmpty(control.Text) ? control.Name : control.Text,
                    Width = control.Width,
                    Height = control.Height,
                    TextWrapping = TextWrapping.WrapWithOverflow,
                    Margin = new Thickness(control.X, control.Y, 0, 0),
                    Foreground = Core.GetColorBrushFromString(control.Color)
                };
                if (control.FontSize > 1) tb.FontSize = control.FontSize;
                dialogCanvas.Children.Add(tb);
            }
        }
    }
}
