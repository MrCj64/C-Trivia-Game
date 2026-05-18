from sqlalchemy import create_engine, MetaData

user = "root"
password = "2005"
host = "127.0.0.1"
port  = 3306
database = "trivia_game_db"

engine = create_engine(
        f"mysql+pymysql://{user}:{password}@{host}:{port}/{database}", echo =True
    )

try:
    meta = MetaData()
    print(f"Connection to the {host} for user {user} created succesfully.")
except Exception as ex:
    print("Connection could not be made due to the following error:\n",ex)


