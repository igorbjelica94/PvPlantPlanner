using System.Data.SQLite;
using PvPlantPlanner.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using PvPlantPlanner.UI.Models;
using System.IO;

namespace PvPlantPlanner.UI.DatabaseRepo
{
    public class DatabaseRepository : IDatabaseRepository
    {
        private readonly SQLiteConnection _connection;
        private readonly string _dbPath;

        public DatabaseRepository()
        {
            // Izračunaj putanju do foldera gde je .exe (bin/Debug ili bin/Release)
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // Kreiraj punu putanju do equipment.db unutar Database foldera
            _dbPath = Path.Combine(baseDir, "Database", "equipment.db");

            // Otvori konekciju
            _connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            _connection.Open();

            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var command = new SQLiteCommand(_connection);

            // Kreiranje tabela ako ne postoje
            command.CommandText = @"
            CREATE TABLE IF NOT EXISTS battery (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                power REAL NOT NULL,
                capacity REAL NOT NULL,
                price REAL NOT NULL,
                cycles INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS transformer (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                power_kva REAL NOT NULL,
                power_factor REAL NOT NULL,
                price REAL NOT NULL
            );";
            command.ExecuteNonQuery();
        }

        #region Battery Implementation
        public int AddBattery(Battery battery)
        {
            const string sql = @"
            INSERT INTO battery (power, capacity, price, cycles)
            VALUES (@power, @capacity, @price, @cycles);
            SELECT last_insert_rowid();";

            using var command = new SQLiteCommand(sql, _connection);
            command.Parameters.AddWithValue("@power", battery.Power);
            command.Parameters.AddWithValue("@capacity", battery.Capacity);
            command.Parameters.AddWithValue("@price", battery.Price);
            command.Parameters.AddWithValue("@cycles", battery.Cycles);

            return Convert.ToInt32(command.ExecuteScalar());
        }

        public bool UpdateBattery(Battery battery)
        {
            const string sql = @"
            UPDATE battery 
            SET power = @power, 
                capacity = @capacity, 
                price = @price, 
                cycles = @cycles
            WHERE id = @id";

            using var command = new SQLiteCommand(sql, _connection);
            command.Parameters.AddWithValue("@id", battery.Id);
            command.Parameters.AddWithValue("@power", battery.Power);
            command.Parameters.AddWithValue("@capacity", battery.Capacity);
            command.Parameters.AddWithValue("@price", battery.Price);
            command.Parameters.AddWithValue("@cycles", battery.Cycles);

            return command.ExecuteNonQuery() > 0;
        }

        public bool DeleteBattery(int id)
        {
            const string sql = "DELETE FROM battery WHERE id = @id";
            using var command = new SQLiteCommand(sql, _connection);
            command.Parameters.AddWithValue("@id", id);
            return command.ExecuteNonQuery() > 0;
        }

        public Battery GetBattery(int id)
        {
            const string sql = "SELECT * FROM battery WHERE id = @id";
            using var command = new SQLiteCommand(sql, _connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Battery
                {
                    Id = reader.GetInt32(0),
                    Power = reader.GetDouble(1),
                    Capacity = reader.GetDouble(2),
                    Price = reader.GetDecimal(3),
                    Cycles = reader.GetInt32(4)
                };
            }
            return null;
        }

        public IEnumerable<Battery> GetAllBatteries()
        {
            var batteries = new List<Battery>();
            const string sql = "SELECT * FROM battery";
            using var command = new SQLiteCommand(sql, _connection);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                batteries.Add(new Battery
                {
                    Id = reader.GetInt32(0),
                    Power = reader.GetDouble(1),
                    Capacity = reader.GetDouble(2),
                    Price = reader.GetDecimal(3),
                    Cycles = reader.GetInt32(4)
                });
            }
            return batteries;
        }
        #endregion

        #region Transformer Implementation
        public int AddTransformer(Transformer transformer)
        {
            const string sql = @"
            INSERT INTO transformer (power_kva, power_factor, price)
            VALUES (@power_kva, @power_factor, @price);
            SELECT last_insert_rowid();";

            using var command = new SQLiteCommand(sql, _connection);
            command.Parameters.AddWithValue("@power_kva", transformer.PowerKVA);
            command.Parameters.AddWithValue("@power_factor", transformer.PowerFactor);
            command.Parameters.AddWithValue("@price", transformer.Price);

            return Convert.ToInt32(command.ExecuteScalar());
        }

        public bool UpdateTransformer(Transformer transformer)
        {
            const string sql = @"
            UPDATE transformer 
            SET power_kva = @power_kva, 
                power_factor = @power_factor, 
                price = @price
            WHERE id = @id";

            using var command = new SQLiteCommand(sql, _connection);
            command.Parameters.AddWithValue("@id", transformer.Id);
            command.Parameters.AddWithValue("@power_kva", transformer.PowerKVA);
            command.Parameters.AddWithValue("@power_factor", transformer.PowerFactor);
            command.Parameters.AddWithValue("@price", transformer.Price);

            return command.ExecuteNonQuery() > 0;
        }

        public bool DeleteTransformer(int id)
        {
            const string sql = "DELETE FROM transformer WHERE id = @id";
            using var command = new SQLiteCommand(sql, _connection);
            command.Parameters.AddWithValue("@id", id);
            return command.ExecuteNonQuery() > 0;
        }

        public Transformer GetTransformer(int id)
        {
            const string sql = "SELECT * FROM transformer WHERE id = @id";
            using var command = new SQLiteCommand(sql, _connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Transformer
                {
                    Id = reader.GetInt32(0),
                    PowerKVA = reader.GetDouble(1),
                    PowerFactor = reader.GetDouble(2),
                    Price = reader.GetDecimal(3)
                };
            }
            return null;
        }

        public IEnumerable<Transformer> GetAllTransformers()
        {
            var transformers = new List<Transformer>();
            const string sql = "SELECT * FROM transformer";
            using var command = new SQLiteCommand(sql, _connection);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                transformers.Add(new Transformer
                {
                    Id = reader.GetInt32(0),
                    PowerKVA = reader.GetDouble(1),
                    PowerFactor = reader.GetDouble(2),
                    Price = reader.GetDecimal(3)
                });
            }
            return transformers;
        }
        #endregion

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}
