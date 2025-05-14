import { db } from '../db/index';
import { sql } from 'drizzle-orm';

// Читаем всех клиентов из таблицы
async function getClients() {
  return db.query.clients.findMany();
}

// Обновляем структуру таблицы reports для изменения столбца client
async function updateReportClientColumn() {
  console.log('🔄 Начинаем обновление столбца client в таблице reports...');
  
  try {
    // Получаем список клиентов
    const clients = await getClients();
    console.log(`✅ Получено ${clients.length} клиентов из таблицы clients`);
    
    // Удаляем временную таблицу, если она существует
    db.run(`DROP TABLE IF EXISTS temp_report_clients;`);
    
    // Создаем таблицу с временными данными отчетов для сохранения значений
    db.run(`
      CREATE TABLE temp_report_clients (
        report_id INTEGER PRIMARY KEY,
        client_name TEXT NOT NULL
      );
    `);
    
    // Сохраняем текущие значения клиентов
    db.run(`
      INSERT INTO temp_report_clients (report_id, client_name)
      SELECT id, client FROM reports WHERE client IS NOT NULL AND client != '';
    `);
    
    console.log('✅ Временные данные о клиентах сохранены');
    
    // Выключаем проверку внешних ключей
    db.run(`PRAGMA foreign_keys=OFF;`);
    
    // Создаем временную таблицу reports с новой структурой
    db.run(`
      CREATE TABLE __new_reports (
        id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
        employee_id INTEGER,
        date TEXT NOT NULL,
        market TEXT,
        contracting_agency TEXT,
        client INTEGER,
        project_brand TEXT,
        media TEXT,
        job_type TEXT,
        comments TEXT,
        hours REAL NOT NULL,
        FOREIGN KEY (employee_id) REFERENCES employees(id) ON DELETE CASCADE,
        FOREIGN KEY (client) REFERENCES clients(id) ON DELETE SET NULL
      );
    `);
    
    // Копируем данные из старой таблицы в новую
    db.run(`
      INSERT INTO __new_reports(id, employee_id, date, market, contracting_agency, client, project_brand, media, job_type, comments, hours)
      SELECT r.id, r.employee_id, r.date, r.market, r.contracting_agency, 
             (SELECT c.id FROM clients c WHERE c.name = temp.client_name LIMIT 1),
             r.project_brand, r.media, r.job_type, r.comments, r.hours
      FROM reports r
      LEFT JOIN temp_report_clients temp ON r.id = temp.report_id;
    `);
    
    // Удаляем старую таблицу
    db.run(`DROP TABLE reports;`);
    
    // Переименовываем новую таблицу
    db.run(`ALTER TABLE __new_reports RENAME TO reports;`);
    
    // Включаем проверку внешних ключей
    db.run(`PRAGMA foreign_keys=ON;`);
    
    // Удаляем временную таблицу
    db.run(`DROP TABLE IF EXISTS temp_report_clients;`);
    
    console.log('✅ Таблица reports успешно обновлена!');
  } catch (error) {
    console.error('❌ Ошибка при обновлении таблицы reports:', error);
  }
}

// Выполняем обновление
updateReportClientColumn(); 