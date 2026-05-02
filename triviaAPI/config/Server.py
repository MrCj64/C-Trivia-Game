
import sqlalchemy as db
from sqlalchemy import create_engine, MetaData

user = "root"
password = "root"
host = "127.0.0.1"
port  = 3306
database = "triviagamebd"

def get_connection():
    engine = db.create_engine(
        f"mysql+pymysql://{user}:{password}@{host}:{port}/{database}", echo =True
    )
    return engine

try:
    conn = get_connection()
    meta = MetaData()
    print(f"Connection to the {host} for user {user} created succesfully.")
except Exception as ex:
    print("Connection could not be made due to the following error:\n",ex)



# Instalar la libreria de sqlalchemy: pip install sqlalchemy ✓ 
# Instalar la liberia de pymysql: pip install mysql ✓
# Crear un motor para conectarse ✓
# Crear las tablas
# Crear las queries
# Crear la API