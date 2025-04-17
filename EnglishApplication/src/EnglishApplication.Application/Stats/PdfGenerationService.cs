// src/EnglishApplication.Application/Stats/PdfGenerationService.cs
using System;
using System.Collections.Generic;
using System.IO;
using EnglishApplication.LearningProgress;
using EnglishApplication.Localization;
using EnglishApplication.Words;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Volo.Abp.DependencyInjection;

namespace EnglishApplication.Stats
{

    [Authorize(Roles = "student, admin")]
    public class PdfGenerationService : ISingletonDependency
    {
        private readonly IStringLocalizer<EnglishApplicationResource> _localizer;

        public PdfGenerationService(
            IStringLocalizer<EnglishApplicationResource> localizer)
        {
            _localizer = localizer;

            // QuestPDF lisans kaydı (Community sürümü)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerateStatisticsPdf(StatDto statDto, List<LearningProgressDto> learningProgress, string userName)
        {
            var accuracyPercentage = statDto.QuestionCount > 0
                ? Math.Round((double)statDto.TrueCount / statDto.QuestionCount * 100)
                : 0;

            // QuestPDF'i yapılandır ve PDF'i oluştur
            return Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Margin(30);

                    // Başlık ve Kullanıcı Bilgileri
                    page.Header().Element(container => ComposeHeader(container));

                    // Ana İçerik
                    page.Content().Element(container =>
                    {
                        ComposeContent(container, statDto, learningProgress, accuracyPercentage, userName);
                    });

                    // Alt Bilgi
                    page.Footer().Element(container => ComposeFooter(container));
                });
            }).GeneratePdf();
        }

        // Başlık Bölümü
        private void ComposeHeader(IContainer container)
        {
            container.Column(column =>
            {
                // Rapor Başlığı
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text(_localizer["StatsReport"])
                            .FontSize(20).Bold().FontColor(Colors.Blue.Medium);
                    });
                });

                // Tarih
                column.Item().Text(text =>
                {
                    text.Span($"{_localizer["GeneratedOn"]}: ")
                        .FontSize(10).FontColor(Colors.Grey.Medium);
                    text.Span($"{DateTime.Now:yyyy-MM-dd HH:mm}")
                        .FontSize(10).FontColor(Colors.Grey.Medium);
                });

                column.Item().PaddingBottom(10);
                column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                column.Item().PaddingBottom(10);
            });
        }

        // Ana İçerik Bölümü
        private void ComposeContent(IContainer container, StatDto statDto, List<LearningProgressDto> learningProgress, double accuracyPercentage, string userName)
        {
            container.Column(column =>
            {
                // Kullanıcı adı
                column.Item().Text(userName)
                    .FontSize(14).Bold().FontColor(Colors.Grey.Darken3);

                column.Item().PaddingVertical(10);

                // İstatistik Özeti Bölümü
                column.Item().Text(_localizer["StatisticsSummary"])
                    .FontSize(16).Bold().FontColor(Colors.Grey.Darken3);

                column.Item().PaddingVertical(10);

                // İstatistik Kartları
                column.Item().Row(row =>
                {
                    // Öğrenilen Kelimeler
                    ComposeStat(row.RelativeItem(), _localizer["LearnedWords"], statDto.LearnedWordCount.ToString(), Colors.Blue.Medium);

                    // Toplam Sorular
                    ComposeStat(row.RelativeItem(), _localizer["TotalQuestions"], statDto.QuestionCount.ToString(), Colors.Blue.Medium);

                    // Doğru Cevaplar
                    ComposeStat(row.RelativeItem(), _localizer["CorrectAnswers"], statDto.TrueCount.ToString(), Colors.Green.Medium);

                    // Yanlış Cevaplar
                    ComposeStat(row.RelativeItem(), _localizer["IncorrectAnswers"], statDto.FalseCount.ToString(), Colors.Red.Medium);
                });

                column.Item().PaddingVertical(10);

                // Doğruluk Analizi Bölümü
                column.Item().Text(_localizer["AccuracyAnalysis"])
                    .FontSize(16).Bold().FontColor(Colors.Grey.Darken3);

                column.Item().PaddingVertical(10);

                column.Item().AlignCenter().Column(c =>
                {
                    c.Item().Text($"{accuracyPercentage}%")
                        .FontSize(36).Bold().FontColor(Colors.Green.Medium);

                    c.Item().Text(_localizer["CorrectAnswersOutOfTotal", statDto.TrueCount, statDto.QuestionCount])
                        .FontSize(12).FontColor(Colors.Grey.Medium);
                });

                column.Item().PaddingVertical(10);

                // İlerleme Tablosu
                column.Item().Text(_localizer["ProgressOverTime"])
                    .FontSize(16).Bold().FontColor(Colors.Grey.Darken3);

                column.Item().PaddingVertical(10);

                // Tablo
                column.Item().Table(table =>
                {
                    // Sütun tanımları
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(120);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    // Tablo Başlığı
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text(_localizer["Date"])
                            .FontSize(12).Bold().FontColor(Colors.Grey.Darken3);

                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text(_localizer["LearnedWords"])
                            .FontSize(12).Bold().FontColor(Colors.Grey.Darken3);

                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text(_localizer["CorrectAnswers"])
                            .FontSize(12).Bold().FontColor(Colors.Grey.Darken3);
                    });

                    // Tablo satırları
                    foreach (var progress in learningProgress)
                    {
                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(progress.Date.ToString("yyyy-MM-dd")).FontSize(10);

                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(progress.LearnedWordsCount.ToString()).FontSize(10).AlignCenter();

                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(progress.CorrectAnswersCount.ToString()).FontSize(10).AlignCenter();
                    }
                });

                // Öğrenilen Kelimeler Bölümü
                if (statDto.LearnedWords != null && statDto.LearnedWords.Count > 0)
                {
                    column.Item().PaddingVertical(20);

                    column.Item().Text(_localizer["LearnedWordsList"])
                        .FontSize(16).Bold().FontColor(Colors.Grey.Darken3);

                    column.Item().PaddingVertical(10);

                    // Fix: Use Element() to pass the IContainer correctly
                    column.Item().Element(container => ComposeLearnedWordsTable(container, statDto.LearnedWords));
                }
            });
        }

        // Öğrenilen Kelimeler Tablosu
        private void ComposeLearnedWordsTable(IContainer container, List<WordDto> learnedWords)
        {
            container.Table(table =>
            {
                // Sütun tanımları
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(50);  // Sıra No
                    columns.RelativeColumn(2);   // İngilizce
                    columns.RelativeColumn(2);   // Türkçe
                });

                // Tablo başlığı
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("#")
                        .FontSize(12).Bold().FontColor(Colors.Grey.Darken3);

                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text(_localizer["EnglishWord"])
                        .FontSize(12).Bold().FontColor(Colors.Grey.Darken3);

                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text(_localizer["TurkishWord"])
                        .FontSize(12).Bold().FontColor(Colors.Grey.Darken3);
                });

                // Kelime listesi
                for (int i = 0; i < learnedWords.Count; i++)
                {
                    var word = learnedWords[i];

                    // Sıra numarası
                    table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text((i + 1).ToString()).FontSize(10).AlignCenter();

                    // İngilizce kelime
                    table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text(word.EnglishWordName).FontSize(10);

                    // Türkçe kelime
                    table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text(word.TurkishWordName).FontSize(10);
                }
            });
        }

        // İstatistik Kartı Oluşturma
        private void ComposeStat(IContainer container, string title, string value, string color)
        {
            container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(column =>
            {
                column.Item().Text(title).FontSize(10).FontColor(Colors.Grey.Medium);
                column.Item().Text(value).FontSize(20).Bold().FontColor(color);
            });
        }

        // Alt Bilgi Oluşturma
        private void ComposeFooter(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                column.Item().PaddingTop(5);

                column.Item().AlignCenter().Text(text =>
                {
                    text.Span($"{_localizer["AppName"]} © {DateTime.Now.Year}")
                        .FontSize(10).FontColor(Colors.Grey.Medium);
                });
            });
        }
    }
}
