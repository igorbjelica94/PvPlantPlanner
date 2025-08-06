CREATE TABLE battery (
    id INTEGER PRIMARY KEY,
    power REAL NOT NULL,
    capacity REAL NOT NULL,
    price INTEGER NOT NULL,
    cycles INTEGER NOT NULL
);

CREATE TABLE transformer (
    id INTEGER PRIMARY KEY,
    power_kva REAL NOT NULL,
    power_factor REAL NOT NULL,
    price INTEGER NOT NULL
);