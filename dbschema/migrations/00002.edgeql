CREATE MIGRATION m13oxwgxweljwrrw7wzqkmpaqe73qsq5d3fnx62fhtwyicasziqmlq
    ONTO m1276apbsjjsntymrkw5mvzdhmpxohurb5ajoemclczxqcifdncada
{
  ALTER TYPE default::Contact {
      DROP PROPERTY password;
      DROP PROPERTY roles;
      DROP PROPERTY username;
  };
  DROP SCALAR TYPE default::Roles;
};
