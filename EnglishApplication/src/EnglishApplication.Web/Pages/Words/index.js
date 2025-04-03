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
                    title: l('Actions'),
                    rowAction: {
                        items:
                            [
                                {
                                    text: l('Edit'),
                                    action: function (data) {
                                        editModal.open({ id: data.record.id });
                                    }
                                },
                                {
                                    text: l('Delete'),
                                    confirmMessage: function (data) {
                                        return l('WordDeletionConfirmationMessage', data.record.name);
                                    },
                                    action: function (data) {
                                        englishApplication.words.word
                                            .delete(data.record.id)
                                            .then(function () {
                                                abp.notify.info(l('SuccessfullyDeleted'));
                                                dataTable.ajax.reload();
                                            });
                                    }
                                }
                            ]
                    }
                },
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
                },
                {
                    title: l('Samples'),
                    data: null,
                    render: function (data) {
                        return '<button class="btn btn-sm btn-primary show-word-samples" data-word-id="' +
                            data.id + '"><i class="fa fa-list"></i> ' + l('Samples') + '</button>';
                    }
                }
            ]
        })
    );


    var createModal = new abp.ModalManager(abp.appPath + 'Words/CreateModal');
    var editModal = new abp.ModalManager(abp.appPath + 'Words/EditModal');
    var wordSamplesModal = new abp.ModalManager({
        viewUrl: abp.appPath + 'WordSamples/WordSamplesModal',
        modalClass: 'WordSamplesModal'
    });
    function openWordSamplesModal(wordId) {
        wordSamplesModal.open({
            wordId: wordId
        });
    }

    // Click handler for show samples button
    $(document).on('click', '.show-word-samples', function (e) {
        e.preventDefault();
        var wordId = $(this).attr('data-word-id');
        console.log("Opening modal for wordId:", wordId);

        wordSamplesModal.open({
            wordId: wordId
        });
    });

    // Click handler for show samples button
    $(document).on('click', '.show-word-samples', function (e) {
        e.preventDefault();
        var wordId = $(this).attr('data-word-id');
        openWordSamplesModal(wordId);
    });

    // Listen to the modal shown event
    $(document).on('abp.modal.open', '.WordSamplesModal', function () {
        // The modal is now open and ready
        console.log('WordSamples modal opened');
    });

    createModal.onResult(function () {
        dataTable.ajax.reload();
    });

    $('#NewWordButton').click(function (e) {
        e.preventDefault();
        createModal.open();
    });


    editModal.onResult(function () {
        dataTable.ajax.reload();
    });


});
