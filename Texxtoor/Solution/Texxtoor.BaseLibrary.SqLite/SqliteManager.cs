using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Texxtoor.BaseLibrary;
using Texxtoor.BusinessLayer;
using Texxtoor.DataModels.Models.Content;

namespace Texxtoor.BaseLibrary.SqLite {
  public class SqliteManager : Manager<SqliteManager> {

    private string _inputfile = ":memory:";
    const string sql = @"CREATE TABLE Elements (
                      Id INTEGER PRIMARY KEY, 
                      Name TEXT NULL, 
                      Title TEXT NULL, 
                      Level INTEGER NOT NULL,
                      Discriminator TEXT NOT NULL, 
                      LocalId TEXT NULL, 
                      Content BLOB NULL,
                      ReadOnly INTEGER NULL, 
                      OrderNr INTEGER NOT NULL, 
                      MimeType TEXT NULL, 
                      Properties TEXT NULL, 
                      Parent_Id INTEGER NULL REFERENCES [Elements]([Id]))"; 
    private string _dbConnection;

    public void CreateSqLiteDatabase() {
      try {
        CreateDb();
        ExecuteNonQuery(sql);
      } catch (Exception ex) {
        Trace.TraceError(ex.Message);
        throw;
      }
    }

    public void CreateSqLiteDatabaseForId(int docId, Func<Element, byte[]> callback) {
      var cnn = new SQLiteConnection(_dbConnection);
      try {
        CreateDb();        
        using (var mycommand = new SQLiteCommand(cnn) {CommandText = sql}) {
          mycommand.ExecuteNonQuery();
        }
        InsertData(docId, callback, cnn);        
      }
      catch (Exception ex) {
        Trace.TraceError(ex.Message);
        throw;
      }
      finally {
        cnn.Close();
      }
    }

    private void CreateDb() {
      try {
        if (File.Exists(_inputfile)) {
          File.Delete(_inputfile);
        }
        SQLiteConnection.CreateFile(_inputfile);
      } catch (Exception ex) {
        Trace.TraceError(ex.Message);
        throw;
      }
    }

    public void CreateSqLiteDatabase(string inputFile) {
      _inputfile = inputFile;
      _dbConnection = String.Format("Data Source={0};Version=3;Journal Mode=Off", _inputfile);
      CreateSqLiteDatabase();
    }

    public DataTable GetDataTable(string sql) {
      var dt = new DataTable();
      try {
        var cnn = new SQLiteConnection(_dbConnection);
        try {
          cnn.Open();
          using (var mycommand = new SQLiteCommand(cnn) { CommandText = sql }) {
            using (var reader = mycommand.ExecuteReader()) {
              dt.Load(reader);
              reader.Close();
            }
          }
        } finally {
          cnn.Close();
          cnn.Dispose();
        }
      } catch (Exception e) {
        throw new Exception(e.Message);
      }
      return dt;
    }

    public int ExecuteNonQuery(string sql) {
      var cnn = new SQLiteConnection(_dbConnection);
      int rowsUpdated;
      try {
        cnn.Open();
        using (var mycommand = new SQLiteCommand(cnn) { CommandText = sql }) {
          rowsUpdated = mycommand.ExecuteNonQuery();
        }
      } catch {
        rowsUpdated = -1;
      } finally {
        cnn.Close();
        cnn.Dispose();
      }
      return rowsUpdated;
    }

    public void InsertData(int docId, Func<Element, byte[]> contentBuilder) {
      var cnn = new SQLiteConnection(_dbConnection);
      if (cnn.State == ConnectionState.Closed) { cnn.Open(); }
      try {
        cnn.Open();
        InsertData(docId, contentBuilder, cnn);
      } finally {
        cnn.Close();
        cnn.Dispose();
      }
    }

    public void InsertData(int docId, Func<Element, byte[]> contentBuilder, SQLiteConnection cnn) {
      const string sq = "SELECT * FROM Elements";
      try {
        using (var da = new SQLiteDataAdapter(sq, cnn)) {
          using (var cb = new SQLiteCommandBuilder(da)) {
            using (da.InsertCommand = cb.GetInsertCommand()) {
              using (var dt = new DataTable()) {
                da.Fill(dt);
                var doc = ProjectManager.Instance.GetOpus(docId, "");
                var lst = GetFlattenElements(doc);
                lst.ForEach(l => dt.Rows.Add(new object[] {
                  l.Id,
                  l.Name,
                  ((l is TitledSnippet) ? ((TitledSnippet) l).Title : l.Name),
                  l.Level,
                  l.WidgetName, // Discriminator
                  l.LocaleId,
                  (contentBuilder != null) ? contentBuilder(l) : l.Content,
                  l.ReadOnly,
                  l.OrderNr,
                  ((l is ImageSnippet) ? ((ImageSnippet) l).MimeType : null),
                  l.Properties,
                  (l.Parent == null) ? 0 : l.Parent.Id
                }));
                da.Update(dt);
              }
            }
            da.SelectCommand.Dispose();
          }
        }
      } catch (Exception ex) {
        Debug.WriteLine(ex.Message);
      }
    }

    internal static List<Element> GetFlattenElements(Element root) {
      var elements = new List<Element> { root };
      FlatElements(elements, root.Children.OrderBy(e => e.OrderNr));
      return elements;
    }

    private static void FlatElements(ICollection<Element> elements, IEnumerable<Element> children) {
      foreach (var item in children) {
        elements.Add(item);
        if (item.HasChildren()) {
          FlatElements(elements, item.Children.OrderBy(e => e.OrderNr));
        }
      }
    }

    public bool ClearTable(string table) {
      try {
        ExecuteNonQuery(String.Format("delete from {0};", table));
        return true;
      } catch {
        return false;
      }
    }


  }
}
