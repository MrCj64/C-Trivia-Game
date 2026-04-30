from sqlalchemy.orm import Session

class CRUD:
    def __init__(self, model):
        self.model = model
    
    def get_all(self, db:Session):
        return db.query(self.model).all()
    
    def get_by_id(self, db:Session, id:int):
        return db.query(self.model).filter(self.model.id == id).first()
    
    def delete_by_id(self, db:Session, id:int):
        obj = self.get_by_id(db, id)
        db.query().delete(obj)
        db.commit()

    def update_by_id(self, db:Session, id:int, data:dict):
        obj = self.get_by_id(db, id)
        db.query().filter(obj).update(data)
        db.commit()
        return obj
    
    def create(self,db:Session, data:dict):
        obj = self.model(**data)
        db.add(obj)
        db.commit()
        db.refresh(obj)
        return obj
        

