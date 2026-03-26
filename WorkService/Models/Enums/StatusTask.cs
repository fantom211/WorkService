namespace WorkService.Models.Enums
{
    public enum StatusTask
    {
        Open = 0, //доступна
        InProgress = 1, //кто-то взял
        TaskReady = 2, //завершена
        Cancelled = 3 //отменена
    }
}
