﻿// The MIT License (MIT)

// Copyright (c) 2015 Ben Abelshausen

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using GTFS.Entities;
using GTFS.Entities.Collections;
using GTFS.Entities.Enumerations;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace GTFS.DB.PostgreSQL.Collections
{
    /// <summary>
    /// Represents a collection of Agencies using an SQLite database.
    /// </summary>
    public class PostgreSQLAgencyCollection : IUniqueEntityCollection<Agency>
    {
        /// <summary>
        /// Holds the connection.
        /// </summary>
        private NpgsqlConnection _connection;

        /// <summary>
        /// Holds the id.
        /// </summary>
        private int _id;

        /// <summary>
        /// Creates a new SQLite GTFS feed.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="id"></param>
        internal PostgreSQLAgencyCollection(NpgsqlConnection connection, int id)
        {
            _connection = connection;
            _id = id;
        }

        /// <summary>
        /// Adds an entity.
        /// </summary>
        /// <param name="entity"></param>
        public void Add(Agency entity)
        {
            string sql = "INSERT INTO agency VALUES (:feed_id, :id, :agency_name, :agency_url, :agency_timezone, :agency_lang, :agency_phone, :agency_fare_url);";
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = sql;
                command.Parameters.Add(new NpgsqlParameter(@"feed_id", DbType.Int64));
                command.Parameters.Add(new NpgsqlParameter(@"id", DbType.String));
                command.Parameters.Add(new NpgsqlParameter(@"agency_name", DbType.String));
                command.Parameters.Add(new NpgsqlParameter(@"agency_url", DbType.String));
                command.Parameters.Add(new NpgsqlParameter(@"agency_timezone", DbType.String));
                command.Parameters.Add(new NpgsqlParameter(@"agency_lang", DbType.String));
                command.Parameters.Add(new NpgsqlParameter(@"agency_phone", DbType.String));
                command.Parameters.Add(new NpgsqlParameter(@"agency_fare_url", DbType.String));

                command.Parameters[0].Value = _id;
                command.Parameters[1].Value = entity.Id;
                command.Parameters[2].Value = entity.Name;
                command.Parameters[3].Value = entity.URL;
                command.Parameters[4].Value = entity.Timezone;
                command.Parameters[5].Value = entity.LanguageCode;
                command.Parameters[6].Value = entity.Phone;
                command.Parameters[7].Value = entity.FareURL;

                command.Parameters.Where(x => x.Value == null).ToList().ForEach(x => x.Value = DBNull.Value);
                command.ExecuteNonQuery();
            }
        }


        public void AddRange(IUniqueEntityCollection<Agency> entities)
        {
            using (var writer = _connection.BeginBinaryImport("COPY agency (feed_id, id, agency_name, agency_url, agency_timezone, agency_lang, agency_phone, agency_fare_url) FROM STDIN (FORMAT BINARY)"))
            {
                foreach (var agency in entities)
                {
                    writer.StartRow();
                    writer.Write(_id, NpgsqlTypes.NpgsqlDbType.Integer);
                    writer.Write(agency.Id, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(agency.Name, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(agency.URL, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(agency.Timezone, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(agency.LanguageCode, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(agency.Phone, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(agency.FareURL, NpgsqlTypes.NpgsqlDbType.Text);
                }
            }
        }

        /// <summary>
        /// Gets the entity with the given id.
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public Agency Get(string entityId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the entity at the given index.
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public Agency Get(int idx)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes the entity with the given id.
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public bool Remove(string entityId)
        {
            string sql = "DELETE FROM agency WHERE FEED_ID = :feed_id AND id = :agency_id;";
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = sql;
                command.Parameters.Add(new NpgsqlParameter(@"feed_id", DbType.Int64));
                command.Parameters.Add(new NpgsqlParameter(@"agency_id", DbType.String));

                command.Parameters[0].Value = _id;
                command.Parameters[1].Value = entityId;

                command.Parameters.Where(x => x.Value == null).ToList().ForEach(x => x.Value = DBNull.Value);
                return command.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// Returns all entities.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Agency> Get()
        {
            string sql = "SELECT id, agency_name, agency_url, agency_timezone, agency_lang, agency_phone, agency_fare_url FROM agency WHERE FEED_ID = :id";
            var parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter(@"id", DbType.Int64));
            parameters[0].Value = _id;

            return new PostgreSQLEnumerable<Agency>(_connection, sql, parameters.ToArray(), (x) =>
            {
                return new Agency()
                {
                    Id = x.GetString(0),
                    Name = x.IsDBNull(1) ? null : x.GetString(1),
                    URL = x.IsDBNull(2) ? null : x.GetString(2),
                    Timezone = x.IsDBNull(3) ? null : x.GetString(3),
                    LanguageCode = x.IsDBNull(4) ? null : x.GetString(4),
                    Phone = x.IsDBNull(5) ? null : x.GetString(5),
                    FareURL = x.IsDBNull(6) ? null : x.GetString(6)
                };
            });
        }

        /// <summary>
        /// Returns entity ids
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetIds()
        {
            #if DEBUG
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            Console.Write($"Fetching agency ids...");
            #endif
            var outList = new List<string>();
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = "SELECT DISTINCT(id) FROM agency";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        outList.Add(Convert.ToString(reader["id"]));
                    }
                }
            }
            #if DEBUG
            stopwatch.Stop();
            Console.WriteLine($" {stopwatch.ElapsedMilliseconds} ms");
            #endif
            return outList;
        }

        /// <summary>
        /// Returns the number of entities.
        /// </summary>
        public int Count
        {
            get
            {
                string sql = "SELECT count(id) FROM agency;";
                using (var command = _connection.CreateCommand())
                {
                    command.CommandText = sql;
                    return int.Parse(command.ExecuteScalar().ToString());
                }
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Agency> GetEnumerator()
        {
            return this.Get().GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.Get().GetEnumerator();
        }

        public bool Update(string entityId, Agency entity)
        {
            string sql = "UPDATE agency SET FEED_ID=:feed_id, id=:id, agency_name=:agency_name, agency_url=:agency_url, agency_timezone=:agency_timezone, agency_lang=:agency_lang, agency_phone=:agency_phone, agency_fare_url=:agency_fare_url WHERE id=:entityId;";
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = sql;
                command.Parameters.Add(new NpgsqlParameter(@"feed_id", DbType.Int64));
                command.Parameters.Add(new NpgsqlParameter(@"id", DbType.String));
                command.Parameters.Add(new NpgsqlParameter(@"agency_name", DbType.String));
                command.Parameters.Add(new NpgsqlParameter(@"agency_url", DbType.String));
                command.Parameters.Add(new NpgsqlParameter(@"agency_timezone", DbType.String));
                command.Parameters.Add(new NpgsqlParameter(@"agency_lang", DbType.String));
                command.Parameters.Add(new NpgsqlParameter(@"agency_phone", DbType.String));
                command.Parameters.Add(new NpgsqlParameter(@"agency_fare_url", DbType.String));
                command.Parameters.Add(new NpgsqlParameter(@"entityId", DbType.String));

                command.Parameters[0].Value = _id;
                command.Parameters[1].Value = entity.Id;
                command.Parameters[2].Value = entity.Name;
                command.Parameters[3].Value = entity.URL;
                command.Parameters[4].Value = entity.Timezone;
                command.Parameters[5].Value = entity.LanguageCode;
                command.Parameters[6].Value = entity.Phone;
                command.Parameters[7].Value = entity.FareURL;
                command.Parameters[8].Value = entityId;

                command.Parameters.Where(x => x.Value == null).ToList().ForEach(x => x.Value = DBNull.Value);
                return command.ExecuteNonQuery() > 0;
            }
        }

        public void RemoveRange(IEnumerable<string> entityIds)
        {
            throw new NotImplementedException();
        }

        public void RemoveAll()
        {
            throw new NotImplementedException();
        }
    }
}