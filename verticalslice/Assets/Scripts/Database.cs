using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Data;
using Mono.Data.Sqlite;
using System;
using System.Security.Cryptography;
using System.Text;

public class Database : MonoBehaviour
{
    public static IDbConnection conn;

    string db_name = "vertical.db";
    
    void Awake()
    {
        conn = new SqliteConnection(string.Format("URI=file:{0}", db_name));
        conn.Open();
    }

    public static List<ArrayList> SendQuery(string query)
    {
        List <ArrayList> res = new List<ArrayList>();

        using (IDbCommand cmd = conn.CreateCommand())
        {
            cmd.CommandText = query;

            using (IDataReader reader = cmd.ExecuteReader())
            {
                int aux = 0;

                while (reader.Read())
                {
                    res.Add(new ArrayList());
                    res[aux] = new ArrayList();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        res[aux].Add(reader[i].ToString());
                    }

                    aux++;
                }
            }
        }

        return res;
    }
}