using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.SqlClient;

namespace Laba4Loz
{
    public record Note(int Id, string Title, string Text, DateTime CreatedAt);

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.MapGet("/health", () =>
                Results.Ok(new
                {
                    status = "ok",
                    time = DateTime.Now
                }));

            app.MapGet("/version", (IConfiguration conf) =>
                Results.Ok(new
                {
                    name = conf["App:Name"] ?? "IsLabApp",
                    version = conf["App:Version"] ?? "0.1.1-autodeploy"
                }));

            var notes = new List<Note>();

            app.MapPost("/api/notes", (Note note) =>
            {
                if (string.IsNullOrWhiteSpace(note.Title))
                    return Results.BadRequest("Title is required");

                var newNote = note with
                {
                    Id = notes.Count + 1,
                    CreatedAt = DateTime.Now
                };

                notes.Add(newNote);

                return Results.Created($"/api/notes/{newNote.Id}", newNote);
            });

            app.MapGet("/api/notes", () => notes);

            app.MapGet("/api/notes/{id}", (int id) =>
            {
                var note = notes.FirstOrDefault(n => n.Id == id);

                return note is not null
                    ? Results.Ok(note)
                    : Results.NotFound();
            });

            app.MapDelete("/api/notes/{id}", (int id) =>
            {
                notes.RemoveAll(n => n.Id == id);
                return Results.NoContent();
            });

            app.MapGet("/db/ping", (IConfiguration conf) =>
            {
                try
                {
                    var connectionString = conf.GetConnectionString("Mssql");

                    if (string.IsNullOrWhiteSpace(connectionString))
                    {
                        return Results.Problem("Connection string is missing");
                    }

                    using var connection = new SqlConnection(connectionString);

                    connection.Open();

                    using var command = new SqlCommand("SELECT 1", connection);

                    var result = command.ExecuteScalar();

                    return Results.Ok(new
                    {
                        status = "ok",
                        db = "connected",
                        result
                    });
                }
                catch (Exception ex)
                {
                    return Results.Json(
                        new
                        {
                            status = "error",
                            message = ex.Message
                        },
                        statusCode: 503
                    );
                }
            });

            app.Run();
        }
    }
}
