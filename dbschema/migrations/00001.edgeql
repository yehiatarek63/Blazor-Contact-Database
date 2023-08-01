CREATE MIGRATION m1276apbsjjsntymrkw5mvzdhmpxohurb5ajoemclczxqcifdncada
    ONTO initial
{
  CREATE SCALAR TYPE default::Roles EXTENDING enum<Admin, User>;
  CREATE SCALAR TYPE default::State EXTENDING enum<Mr, Mrs, Miss, Dr, Prof>;
  CREATE TYPE default::Contact {
      CREATE REQUIRED PROPERTY date_of_birth: std::datetime;
      CREATE REQUIRED PROPERTY description: std::str;
      CREATE REQUIRED PROPERTY email: std::str;
      CREATE REQUIRED PROPERTY first_name: std::str;
      CREATE REQUIRED PROPERTY last_name: std::str;
      CREATE REQUIRED PROPERTY marriage_status: std::bool;
      CREATE REQUIRED PROPERTY password: std::str;
      CREATE REQUIRED PROPERTY roles: default::Roles;
      CREATE REQUIRED PROPERTY title: default::State;
      CREATE REQUIRED PROPERTY username: std::str {
          CREATE CONSTRAINT std::exclusive;
      };
  };
};
