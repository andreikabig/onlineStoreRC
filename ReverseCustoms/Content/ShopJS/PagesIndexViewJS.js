$(function () {
    /* Подтверждение и удаление страницы*/
    $("a.delete").click(function () {
        if (!confirm("Вы действительно хотите удалить эту страницу?")) return false;
    });
    /*===========================================*/

    /*Скрипт сортировки*/
    $("table#pages tbody").sortable({
        items: "tr:not(.home)",
        placeholder: "ui-state-highlitght",
        update: function () {
            var ids = $("table#pages tbody").sortable("serialize");
            var url = "/Admin/Pages/ReorderPages"

            $.post(url, ids, function (data) {
            });
        }
    });

});