using SQLite;
using CityExplorer.Models;

namespace CityExplorer.Services;

/// <summary>
/// SQLite andmebaasiteenus (CRUD operatsioonid).
/// Registreeritakse MauiProgram.cs-is Singleton'ina.
/// </summary>
public class DatabaseService
{
    private SQLiteAsyncConnection? _database;
    private const string DatabaseFilename = "cityexplorer.db3";

    private static string DatabasePath =>
        Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);

    /// <summary>
    /// Lähtestab andmebaasi ühenduse ja loob tabelid (kui neid veel pole).
    /// </summary>
    public async Task InitAsync()
    {
        if (_database is not null)
            return;

        _database = new SQLiteAsyncConnection(
            DatabasePath,
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

        await _database.CreateTableAsync<Place>();
    }

    /// <summary>
    /// Tagastab kõik lemmikuks märgitud kohad andmebaasist.
    /// </summary>
    public async Task<List<Place>> GetFavoritesAsync()
    {
        await InitAsync();
        return await _database!.Table<Place>()
            .Where(p => p.IsFavorite)
            .ToListAsync();
    }

    /// <summary>
    /// Tagastab KÕIK kohad andmebaasist (Admin vaate jaoks).
    /// </summary>
    public async Task<List<Place>> GetAllPlacesAsync()
    {
        await InitAsync();
        return await _database!.Table<Place>().ToListAsync();
    }

    /// <summary>
    /// Salvestab koha andmebaasi (INSERT kui uus, UPDATE kui olemasolev).
    /// </summary>
    public async Task<int> SavePlaceAsync(Place place)
    {
        await InitAsync();
        return place.Id != 0
            ? await _database!.UpdateAsync(place)
            : await _database!.InsertAsync(place);
    }

    /// <summary>
    /// Kustutab koha andmebaasist (DELETE).
    /// </summary>
    public async Task<int> DeletePlaceAsync(Place place)
    {
        await InitAsync();
        return await _database!.DeleteAsync(place);
    }
}