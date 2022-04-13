CREATE TABLE Cemco_dw.upload.ClassCodeManagement (
ID           INT  IDENTITY(1,1) PRIMARY KEY,
ClassCodeID        INT,
ProductCodeId        INT,
LocationId            INT,
Active                INT)
--------------

CREATE TABLE Cemco_dw.upload.DisplayMonths (
ID           INT  IDENTITY(1,1) PRIMARY KEY,
Month        INT,
Year         INT,
Active       INT
)

-----------

CREATE TABLE Cemco_dw.upload.TransferFreight (
ID                    INT  IDENTITY(1,1) PRIMARY KEY,
TransferFromId        INT,
TransferToId          INT,
ProductCode           VARCHAR(20),
TransferCost          float )



----------
CREATE TABLE Cemco_dw.upload.AddedFreight(
ID                       INT  IDENTITY(1,1) PRIMARY KEY,
POLocationId           INT ,
POWarehouseId          INT,
POCarrierId            INT,
VendorId               INT,
CWT                    VARCHAR(20),
TruckLoad              VARCHAR(20)
)
ALTER TABLE Upload.AddedFreight
ADD CONSTRAINT FK_Location
FOREIGN KEY (POLocationId) REFERENCES Location(LocationId);