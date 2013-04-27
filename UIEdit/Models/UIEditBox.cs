namespace UIEdit.Models {
    public class UIEditBox : UIControl {
        public bool ReadOnly { get; set; }
        public string FileName { get; set; }
        public string Text { get; set; }
        public string Color { get; set; }
        public UIResourceFrameImage FrameImage { get; set; }
    }
}