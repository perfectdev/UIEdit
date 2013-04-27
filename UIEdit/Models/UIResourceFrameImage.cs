namespace UIEdit.Models {
    public class UIResourceFrameImage : UIResource {
        public string FileName { get; set; }

        public UIResourceFrameImage() {
            Type = UIResourceType.FrameImage;
        }
    }
}
