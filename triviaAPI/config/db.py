from sqlalchemy import create_engine, MetaData

user = "root"
password = "root"
host = "127.0.0.1"
port  = 3306
database = "triviagamebd"

engine = create_engine(
        f"mysql+pymysql://{user}:{password}@{host}:{port}/{database}", echo =True
    )

try:
    meta = MetaData()
    print(f"Connection to the {host} for user {user} created succesfully.")
except Exception as ex:
    print("Connection could not be made due to the following error:\n",ex)


