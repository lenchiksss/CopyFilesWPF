namespace CopyFilesWPF.Presenter
{
    public interface IMainWindowPresenter
    {
        void CopyButtonClick();

        void ChooseFileToButtonClick(string path);

        void ChooseFileFromButtonClick(string path);
    }
}