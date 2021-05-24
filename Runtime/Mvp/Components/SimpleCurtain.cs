namespace Behc.Mvp.Components
{
    public class SimpleCurtain : Curtain
    {
        public override void Setup(bool visible, int order)
        {
            gameObject.SetActive(visible);
        }

        public override void Show(int order)
        {
            gameObject.SetActive(true);
        }

        public override void Switch(int newOrder, int previousOrder)
        {
            gameObject.SetActive(true);
        }

        public override void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}