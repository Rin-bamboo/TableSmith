namespace TableSmith.Common.Enum
{
    public static class CommonEnum
    {
        /// <summary>
        /// ダイアログの種類を定義します。
        /// </summary>
        public enum EnumDialogType
        {
            None,
            Information,
            Warning,
            Error,
            Question
        }
        /// <summary>
        /// ダイアログに表示するボタンの種類を定義します。
        /// </summary>
        public enum EnumDialogButtons
        {
            Ok,
            OkCancel,
            YesNo,
            YesNoCancel
        }
        /// <summary>
        /// ダイアログで選択された結果を表す列挙体です。
        /// </summary>
        public enum EnumDialogResult
        {
            None,
            Ok,
            Cancel,
            Yes,
            No
        }

    }
}
