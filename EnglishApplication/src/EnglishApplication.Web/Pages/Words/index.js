$(function () {
    var l = abp.localization.getResource('EnglishApplication');
    var currentUserId = abp.currentUser.id;

    // Check word set status on page load
    checkWordSetStatus();
    function checkWordSetStatus() {
        return englishApplication.userSettings.userSetting.getIsWordSetLoad(currentUserId)
            .then(function (isLoaded) {
                if (isLoaded) {
                    $('#AddTop100WordButton').prop('disabled', true);
                }
                return isLoaded;
            })
            .catch(function (error) {
                console.error("Error checking word set status:", error);
                return false;
            });
    }


    // Add handler for AddTop100WordButton
    $('#AddTop100WordButton').click(function (e) {
        e.preventDefault();

        // Check if button is disabled
        if ($(this).prop('disabled')) {
            abp.message.warn(l('WordSetAlreadyAdded'));
            return;
        }

        abp.notify.info(l('AddingCommonWords'));

        // Call the service to add words and update user setting
        englishApplication.words.word.addWordSetByUserId(currentUserId)
            .then(function (result) {
                if (result) {
                    abp.notify.success(l('CommonWordsAddedSuccess'));
                    $('#AddTop100WordButton').prop('disabled', true);
                    dataTable.ajax.reload();
                }
            })
            .catch(function (error) {
                console.error("Error adding common words:", error);
                abp.notify.error(l('ErrorAddingCommonWords'));
            })

    });


    var dataTable = $('#WordsTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [[1, "asc"]], // Changed to column 1 (EnglishWordName) since column 0 is Actions
            searching: false,
            scrollX: true,
            ajax: function (requestData, callback, settings) {
                var input = {
                    maxResultCount: requestData.length,
                    skipCount: requestData.start,
                    sorting: requestData.columns[requestData.order[0].column].data + " " +
                        requestData.order[0].dir
                };

                englishApplication.words.word.getWordDetailsByUserId(input, currentUserId)
                    .then(function (result) {
                        // Process the result for DataTables
                        callback({
                            recordsTotal: result.totalCount,
                            recordsFiltered: result.totalCount,
                            data: result.items
                        });
                    })
                    .catch(function (error) {
                        console.error("Error loading word details:", error);
                        abp.notify.error(l('ErrorLoadingWordDetails'));
                        callback({
                            recordsTotal: 0,
                            recordsFiltered: 0,
                            data: []
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
                                        return l('WordDeletionConfirmationMessage', data.record.englishWordName);
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

    // Click handler for show samples button (removed duplicate handler)
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