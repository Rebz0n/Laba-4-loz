using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Laba4Loz
{
    // Модель данных
    public record Note(int Id, string Title, string Text, DateTime CreatedAt);

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Minimal API - no MVC controllers required for this lab
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.MapGet("/health", () => Results.Ok(new { status = "ok", time = DateTime.Now }));

            app.MapGet("/version", (IConfiguration conf) =>
                Results.Ok(new { name = conf["App:Name"], version = conf["App:Version"] }));

            var notes = new List<Note>();

            app.MapPost("/api/notes", (Note note) => {
                if (string.IsNullOrEmpty(note.Title)) return Results.BadRequest("Title is required");
                var newNote = note with { Id = notes.Count + 1, CreatedAt = DateTime.Now };
                notes.Add(newNote);
                return Results.Created($"/api/notes/{newNote.Id}", newNote);
            });

            app.MapGet("/api/notes", () => notes);

            app.MapGet("/api/notes/{id}", (int id) =>
                notes.FirstOrDefault(n => n.Id == id) is Note n ? Results.Ok(n) : Results.NotFound());

            app.MapDelete("/api/notes/{id}", (int id) => {
                notes.RemoveAll(n => n.Id == id);
                return Results.NoContent();
            });

            app.MapGet("/db/ping", (IConfiguration conf) => {
                var connectionString = conf.GetConnectionString("Mssql");
                if (string.IsNullOrEmpty(connectionString))
                    return Results.Problem("Connection string is missing");

                return Results.Json(new { status = "error", message = "SQL Server not reachable yet" }, statusCode: 503);
            });

            app.Run();
        }
    }
}
