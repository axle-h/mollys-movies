using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MolliesMovies.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Genre",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 191, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genre", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LocalMovie",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Source = table.Column<string>(maxLength: 191, nullable: false),
                    ImdbCode = table.Column<string>(maxLength: 191, nullable: false),
                    Title = table.Column<string>(maxLength: 191, nullable: false),
                    Year = table.Column<int>(nullable: false),
                    ThumbPath = table.Column<string>(maxLength: 255, nullable: true),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DateScraped = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalMovie", x => x.Id);
                    table.UniqueConstraint("AK_LocalMovie_ImdbCode", x => x.ImdbCode);
                });

            migrationBuilder.CreateTable(
                name: "Movie",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MetaSource = table.Column<string>(maxLength: 191, nullable: false),
                    ImdbCode = table.Column<string>(maxLength: 191, nullable: false),
                    Title = table.Column<string>(maxLength: 191, nullable: false),
                    Language = table.Column<string>(maxLength: 191, nullable: false),
                    Year = table.Column<int>(nullable: false),
                    Rating = table.Column<decimal>(type: "DECIMAL(3, 1)", nullable: false),
                    Description = table.Column<string>(maxLength: 4096, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movie", x => x.Id);
                    table.UniqueConstraint("AK_Movie_ImdbCode", x => x.ImdbCode);
                });

            migrationBuilder.CreateTable(
                name: "Scrape",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: true),
                    Success = table.Column<bool>(nullable: false),
                    LocalMovieCount = table.Column<int>(nullable: false),
                    MovieCount = table.Column<int>(nullable: false),
                    TorrentCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scrape", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DownloadedMovie",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MovieImdbCode = table.Column<string>(maxLength: 191, nullable: false),
                    LocalMovieImdbCode = table.Column<string>(maxLength: 191, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadedMovie", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DownloadedMovie_LocalMovie_LocalMovieImdbCode",
                        column: x => x.LocalMovieImdbCode,
                        principalTable: "LocalMovie",
                        principalColumn: "ImdbCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DownloadedMovie_Movie_MovieImdbCode",
                        column: x => x.MovieImdbCode,
                        principalTable: "Movie",
                        principalColumn: "ImdbCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovieGenre",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MovieId = table.Column<int>(nullable: false),
                    GenreId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieGenre", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovieGenre_Genre_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genre",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovieGenre_Movie_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovieSource",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MovieId = table.Column<int>(nullable: false),
                    Source = table.Column<string>(maxLength: 191, nullable: false),
                    SourceUrl = table.Column<string>(maxLength: 255, nullable: false),
                    SourceId = table.Column<string>(maxLength: 191, nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DateScraped = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieSource", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovieSource_Movie_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransmissionContext",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MovieId = table.Column<int>(nullable: false),
                    TorrentId = table.Column<int>(nullable: false),
                    ExternalId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    MagnetUri = table.Column<string>(maxLength: 4096, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransmissionContext", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransmissionContext_Movie_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScrapeSource",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ScrapeId = table.Column<int>(nullable: false),
                    Source = table.Column<string>(maxLength: 191, nullable: false),
                    Type = table.Column<string>(maxLength: 191, nullable: false),
                    Success = table.Column<bool>(nullable: false),
                    Error = table.Column<string>(maxLength: 4096, nullable: true),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: true),
                    MovieCount = table.Column<int>(nullable: false),
                    TorrentCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScrapeSource", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScrapeSource_Scrape_ScrapeId",
                        column: x => x.ScrapeId,
                        principalTable: "Scrape",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Torrent",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MovieId = table.Column<int>(nullable: false),
                    Url = table.Column<string>(maxLength: 255, nullable: false),
                    Hash = table.Column<string>(maxLength: 255, nullable: false),
                    Quality = table.Column<string>(maxLength: 191, nullable: false),
                    Type = table.Column<string>(maxLength: 191, nullable: false),
                    SizeBytes = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Torrent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Torrent_MovieSource_MovieId",
                        column: x => x.MovieId,
                        principalTable: "MovieSource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransmissionContextStatus",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TransmissionContextId = table.Column<int>(nullable: false),
                    Status = table.Column<string>(maxLength: 191, nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransmissionContextStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransmissionContextStatus_TransmissionContext_TransmissionCo~",
                        column: x => x.TransmissionContextId,
                        principalTable: "TransmissionContext",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DownloadedMovie_LocalMovieImdbCode",
                table: "DownloadedMovie",
                column: "LocalMovieImdbCode");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadedMovie_MovieImdbCode",
                table: "DownloadedMovie",
                column: "MovieImdbCode");

            migrationBuilder.CreateIndex(
                name: "IX_Genre_Name",
                table: "Genre",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocalMovie_ImdbCode",
                table: "LocalMovie",
                column: "ImdbCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocalMovie_Source",
                table: "LocalMovie",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_Movie_ImdbCode",
                table: "Movie",
                column: "ImdbCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Movie_Language",
                table: "Movie",
                column: "Language");

            migrationBuilder.CreateIndex(
                name: "IX_Movie_Rating",
                table: "Movie",
                column: "Rating");

            migrationBuilder.CreateIndex(
                name: "IX_Movie_Title",
                table: "Movie",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_MovieGenre_GenreId",
                table: "MovieGenre",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_MovieGenre_MovieId",
                table: "MovieGenre",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_MovieSource_DateCreated",
                table: "MovieSource",
                column: "DateCreated");

            migrationBuilder.CreateIndex(
                name: "IX_MovieSource_MovieId",
                table: "MovieSource",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_MovieSource_Source",
                table: "MovieSource",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_ScrapeSource_ScrapeId",
                table: "ScrapeSource",
                column: "ScrapeId");

            migrationBuilder.CreateIndex(
                name: "IX_Torrent_MovieId",
                table: "Torrent",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_Torrent_Quality",
                table: "Torrent",
                column: "Quality");

            migrationBuilder.CreateIndex(
                name: "IX_Torrent_Type",
                table: "Torrent",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_TransmissionContext_ExternalId",
                table: "TransmissionContext",
                column: "ExternalId");

            migrationBuilder.CreateIndex(
                name: "IX_TransmissionContext_MovieId",
                table: "TransmissionContext",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_TransmissionContextStatus_TransmissionContextId",
                table: "TransmissionContextStatus",
                column: "TransmissionContextId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DownloadedMovie");

            migrationBuilder.DropTable(
                name: "MovieGenre");

            migrationBuilder.DropTable(
                name: "ScrapeSource");

            migrationBuilder.DropTable(
                name: "Torrent");

            migrationBuilder.DropTable(
                name: "TransmissionContextStatus");

            migrationBuilder.DropTable(
                name: "LocalMovie");

            migrationBuilder.DropTable(
                name: "Genre");

            migrationBuilder.DropTable(
                name: "Scrape");

            migrationBuilder.DropTable(
                name: "MovieSource");

            migrationBuilder.DropTable(
                name: "TransmissionContext");

            migrationBuilder.DropTable(
                name: "Movie");
        }
    }
}
