use rusqlite::{Connection, Result, Error, Row};

fn main() -> Result<()> {
    let mut args = std::env::args().into_iter().skip(1);
    
    let conn = Connection::open(args.next().expect("Specify a database"))?;
    let table_name = args.next().or(Some("Данные".to_owned())).unwrap();

    println!("SELECT * FROM {table_name};");
    let mut stmt = conn.prepare(&format!("SELECT * FROM {table_name};"))?;

    let columns = stmt.column_count();
    let mut rows = stmt.query([])?;
    
    while let Some(row) = rows.next()? {
        let row_id = row.get::<usize, i32>(0)?;
        let mut update_query = format!("UPDATE {table_name}\nSET ");
        
        for i in 1..columns {
            let num = read_number(row, i).unwrap();

            let delimeter = if i == 1 { "" } else { "," };
            let line = format!("{delimeter}\n\"{i}\" = {num}");
            update_query.push_str(&line);
        }

        let update_query = format!("{update_query}\nWHERE Эпоха = {row_id};");
        let mut update = conn.prepare(&update_query)?;
        update.execute([])?;
    }

    Ok(())
}

fn read_number(row: &Row, column: usize) -> Option<f64> {
    match row.get::<usize, String>(column) {
        Ok(text) => match text.trim().replace(',', ".").parse::<f64>() {
            Ok(f) => Some(f),
            _ => panic!("In column {column} of {row:?} is not a number"),
        },

        Err(Error::InvalidColumnType(..)) => Some(
            row.get(column)
                .expect(&format!("In column {column} of {row:?} is neither text nor number"))
        ),

        Err(Error::InvalidColumnIndex(..)) => None,

        _ => unreachable!(),
    }
}