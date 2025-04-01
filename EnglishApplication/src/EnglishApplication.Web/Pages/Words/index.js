$(function () {
    var l = abp.localization.getResource('EnglishApplication');
    var currentUserId = abp.currentUser.id;

    var dataTable = $('#WordsTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [[1, "asc"]],
            searching: false,
            scrollX: true,
            ajax: function (requestData, callback, settings) {
                // Call the service with userId parameter
                englishApplication.words.word.getWordDetailsByUserId(currentUserId)
                    .then(function (result) {
                        // Process the result for DataTables
                        callback({
                            recordsTotal: result.length,
                            recordsFiltered: result.length,
                            data: result
                        });
                    });
            },
            columnDefs: [
                {
                    title: l('EnglishWordName'),
                    data: "englishWordName"
                },
                {
                    title: l('TurkishWordName'),
                    data: "turkishWordName"
                },
                {
                    title: l('Picture'),
                    data: "picture",
                    render: function (data) {
                        if (data) {
                            return '<img src="' + data + '" alt="Word Picture" style="max-height: 50px;" />';
                        }
                        return 'N/A';
                    }
                },
                {
                    title: l('TrueCount'),
                    data: "trueCount"
                },
                {
                    title: l('NextDate'),
                    data: "nextDate",
                    render: function (data) {
                        if (data) {
                            return luxon
                                .DateTime
                                .fromISO(data, {
                                    locale: abp.localization.currentCulture.name
                                }).toLocaleString(luxon.DateTime.DATETIME_SHORT);
                        }
                        return 'N/A';
                    }
                },
                {
                    title: l('IsLearn'),
                    data: "isLearn",
                    render: function (data) {
                        return data ? '<i class="fas fa-check text-success"></i>' : '<i class="fas fa-times text-danger"></i>';
                    }
                }
            ]
        })
    );


    var createModal = new abp.ModalManager(abp.appPath + 'Words/CreateModal');

    createModal.onResult(function () {
        dataTable.ajax.reload();
    });

    $('#NewWordButton').click(function (e) {
        e.preventDefault();
        createModal.open();
    });






});
