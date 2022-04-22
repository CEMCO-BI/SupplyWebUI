import { HttpClient, HttpRequest, HttpEventType, HttpResponse, HttpHeaders, HttpParams } from '@angular/common/http'
import { Component, ElementRef, Inject, Input, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import * as XLSX from 'xlsx';
import { Toast, ToastrService } from 'ngx-toastr';
import { GlobalConstants } from '../common/global-constant';
import { ActivatedRoute } from '@angular/router';
import { UploadService } from '../service/upload.service';
import { AgGridAngular } from 'ag-grid-angular';

@Component({
  selector: 'app-upload-file',
  templateUrl: './upload-file.component.html',
  styleUrls: ['./upload-file.component.css']
})

export class UploadFileComponent implements OnInit {
  errorlist: {
    LineNumber: number, ErrorMessage: string, RowData: { Id: number, Year: number, Month: number, Location: string, Amount: number }
  }[];
  message: string;
  sheet: [][];
  header: [][];
  data: [][];
  x: [][];
  baseUrl: string;
  fileName: string;
  @ViewChild('labelImport', { static: true })
  @ViewChild('file', { static: false })
  InputVar: ElementRef;
  displayerrors: boolean = false;

  //type of file
  typeOfFile: string = "F_01";
  typo: string = "F_01";
  displayDatePicker: boolean = false;
  displayBrowseFile: boolean = true;
  displayMarginTables: boolean = false;
  displayGrid: boolean = false;
  to: string = null;
  from: string = null;
  selectedFile: any;
  isNewRowAdded: boolean = false;

  //Margin Tables constants
  addedFreightrowData: any;
  transferFreightrowData: any;
  classCodeManagementrowData: any;
  displayMonthsrowData: any;
  AddedFreightcolumnDefs: any;
  TransferFreightcolumnDefs: any;
  DisplayMonthscolumnDefs: any;
  ClassCodeManagementcolumnDefs: any;

  @ViewChild('addedFreightGrid', { static: false }) addedFreightGrid: AgGridAngular;
  @ViewChild('transferFreightGrid', { static: false }) transferFreightGrid: AgGridAngular;
  @ViewChild('classCodeManagementGrid', { static: false }) classCodeManagementGrid: AgGridAngular;
  @ViewChild('displayMonthsGrid', { static: false }) displayMonthsGrid: AgGridAngular;
  private addedFreightgridApi;
  private addedFreightgridColumnApi;
  private transferFreightgridApi;
  private transferFreightgridColumnApi;
  private classCodeManagementgridApi;
  private classCodeManagementColumnApi;
  private displayMonthsgridApi;
  private displayMonthsColumnApi;
  private locations: object = {};
  private warehouse: object = {};
  private warehouseIND: object = {};
  private warehousePIT: object = {};
  private warehouseDEN: object = {};
  private warehouseFTW: object = {};
  private carrier: object = {};
  private vendor: object = {};
  private productCode: object = {};
  private classCode: object = {};
  active = {
    1: "True",
    0: "False"
  }
  month = {
    1: "January", 2: "February", 3:"March", 4:"April", 5:"May", 6:"June", 7:"July", 8:"August", 9:"September", 10:"October", 11:"November", 12:"December"
  };

  constructor(private http: HttpClient, @Inject('BASE_URL') baseUrl: string, private toastr: ToastrService, private route: ActivatedRoute, private uploadService: UploadService) {
    this.baseUrl = baseUrl;
    this.getLocations();
    this.getAllWarehouse();
    this.getINDWarehouse();
    this.getPITWarehouse();
    this.getDENWarehouse();
    this.getFTWWarehouse();
    this.getCarrier();
    this.getVendor();
    this.getProductCode();
    this.getClassCode();
  }

  ngOnInit(): void {

    if (this.route.snapshot.params.typo == 'F_02' || this.route.snapshot.params.typo == 'F_03' || this.route.snapshot.params.typo == 'F_01' || this.route.snapshot.params.typo == 'F_04') {
      this.typeOfFile = this.route.snapshot.params.typo;
      
    }
    if (this.typeOfFile == 'F_02') {
      this.displayDatePicker = true;
    }

    if (this.typeOfFile == GlobalConstants.F_04) {
      this.displayBrowseFile = false;
      this.displayMarginTables = true;

    //Creating column defs
    this.createAddedFreightColumnDefs();
    this.createTransferFreightcolumnDefs();
    this.createClassCodeManagementcolumnDefs();
    this.createDisplayMonthscolumnDefs();

    //Get Data from db
    this.getAddedFreightDetails();
    this.getTransferFreightDetails();
    this.getClassCodeManagementDetails();
    this.getDisplayMonthsDetails();
      
    }
    else {
      this.displayBrowseFile = true;
      this.displayMarginTables = false;
    }
    
  };
  extractValues(mappings) {
    return Object.keys(mappings);
  }

  //Get locations from database for location columns
  getLocations() {
    return this.http.get('./GetLocations').subscribe(
      data => {
        var parsedArray = JSON.parse(JSON.stringify(data));
        var obj = parsedArray.reduce((acc, i) => {
          acc[i.locationId] = i.locationCode;
          return acc;
        }, {});
        this.locations = obj;
        this.ngOnInit();
      }
    )

  }

  //Get warehouse from database for warehouse columns
  getAllWarehouse() {
    return this.http.get('./GetAllWarehouse').subscribe(
      data => {
        var parsedArray = JSON.parse(JSON.stringify(data));
        var obj = parsedArray.reduce((acc, i) => {
          acc[i.warehouseId] = i.abbr;
          return acc;
        }, {});
        this.warehouse = obj;
        this.ngOnInit();
      }
    )

  }

  getINDWarehouse() {
    return this.http.get('./GetINDWarehouse').subscribe(
      data => {
        var parsedArray = JSON.parse(JSON.stringify(data));
        var obj = parsedArray.reduce((acc, i) => {
          acc[i.warehouseId] = i.abbr;
          return acc;
        }, {});
        this.warehouseIND = obj;
        this.ngOnInit();
      }
    )

  }

  getPITWarehouse() {
    return this.http.get('./GetPITWarehouse').subscribe(
      data => {
        var parsedArray = JSON.parse(JSON.stringify(data));
        var obj = parsedArray.reduce((acc, i) => {
          acc[i.warehouseId] = i.abbr;
          return acc;
        }, {});
        this.warehousePIT = obj;
        this.ngOnInit();
      }
    )

  }

  getDENWarehouse() {
    return this.http.get('./GetDENWarehouse').subscribe(
      data => {
        var parsedArray = JSON.parse(JSON.stringify(data));
        var obj = parsedArray.reduce((acc, i) => {
          acc[i.warehouseId] = i.abbr;
          return acc;
        }, {});
        this.warehouseDEN = obj;
        this.ngOnInit();
      }
    )

  }

  getFTWWarehouse() {
    return this.http.get('./GetFTWWarehouse').subscribe(
      data => {
        var parsedArray = JSON.parse(JSON.stringify(data));
        var obj = parsedArray.reduce((acc, i) => {
          acc[i.warehouseId] = i.abbr;
          return acc;
        }, {});
        this.warehouseFTW = obj;
        this.ngOnInit();
      }
    )

  }

  //Get carrier from database for carrier columns
  getCarrier() {
    return this.http.get('./GetCarrier').subscribe(
      data => {
        var parsedArray = JSON.parse(JSON.stringify(data));
        var obj = parsedArray.reduce((acc, i) => {
          acc[i.carrierId] = i.description;
          return acc;
        }, {});
        this.carrier = obj;
        this.ngOnInit();
      }
    )
  }

  //Get vendor from database for vendor columns
  getVendor() {
    return this.http.get('./GetVendor').subscribe(
      data => {
        var parsedArray = JSON.parse(JSON.stringify(data));
        var obj = parsedArray.reduce((acc, i) => {
          acc[i.vendorId] = i.checkName;
          return acc;
        }, {});
        this.vendor = obj;
        this.ngOnInit();
      }
    )
  }

  //Get productCodes from database for Product Code columns
  getProductCode() {
    return this.http.get('./GetProductCode').subscribe(
      data => {
        var parsedArray = JSON.parse(JSON.stringify(data));
        var obj = parsedArray.reduce((acc, i) => {
          acc[i.partId] = i.partNo;
          return acc;
        }, {});
        this.productCode = obj;
        this.ngOnInit();
      }
    )
  }

  //Get class code from database for class code columns
  getClassCode() {
    return this.http.get('./GetClassCode').subscribe(
      data => {
        var parsedArray = JSON.parse(JSON.stringify(data));
        var obj = parsedArray.reduce((acc, i) => {
          acc[i.classCodeId] = i.code;
          return acc;
        }, {});
        this.classCode = obj;
        this.ngOnInit();
      }
    )
  }

  cwtFormatter(currency, sign) {
  var sansDec = currency.toFixed(2);
  var formatted = sansDec.replace(/\B(?=(\d{3})+(?!\d))/g, ',');
  return sign + `${formatted}`;
  }

  //Added Freight
  createAddedFreightColumnDefs() {
    this.AddedFreightcolumnDefs = [
      {
        field: "poLocationId", headerName: "PO Location", width: "90", editable: true, cellEditor: 'agSelectCellEditor',
        cellEditorParams: {
          values: this.extractValues(this.locations),
        }
        , refData: this.locations
        , required: true
        
      },
      {
        field: "poWarehouseId", headerName: "PO Warehouse", width: "110", editable: true, cellEditor: 'agSelectCellEditor',
        //cellEditorParams: {
        //  values: this.extractValues(this.warehouse),
        //}
        cellEditorParams: (params) => {
          var selectedLocationId = params.data.poLocationId;
          if (selectedLocationId == 1) {
            return {
              values: this.extractValues(this.warehouseIND)
            };
          }
          else if (selectedLocationId == 2) {
            return {
              values: this.extractValues(this.warehousePIT)
            };
          }
          else if (selectedLocationId == 33) {
            return {
              values: this.extractValues(this.warehouseDEN)
            };
          }
          else {
            return {
              values: this.extractValues(this.warehouseFTW)
            };
          }
        }
        , refData: this.warehouse
        ,required: true
      },
      {
        field: "poCarrierId", headerName: "PO Carrier", width: "90", editable: true, cellEditor: 'agSelectCellEditor',
        cellEditorParams: {
          values: this.extractValues(this.carrier),
        }
        , refData: this.carrier
        , required: true
      },
      {
        field: "vendorId", headerName: "Vendor", width: "125", editable: true, cellEditor: 'agSelectCellEditor',
        cellEditorParams: {
          values: this.extractValues(this.vendor),
        }
        , refData: this.vendor
        , required: true //TODO:type ahead search
      },
      {
        field: "cwt", headerName: "Added Freight/CWT", width: "140"
        , required: true
        //, valueFormatter: params => this.cwtFormatter(params.data.cwt, '$')
        //, valueFormatter: (params) => {
        //  if (params.data.cwt) {
        //    return this.cwtFormatter(params.data.cwt, '$');
        //  }
        //  return "";
        //}
        , valueFormatter: params => {
          return '$' + this.formatTruckLoad(params.data.cwt);
        }
        , editable: true
      },
      {
        field: "truckLoad", headerName: "$/Truckload", width: "100"
        , required: true
        //, valueFormatter: (params) => {
        //  if (params.data.truckLoad) {
        //    return '$' + this.formatTruckLoad(params.data.truckLoad);
        //  }
        //  return "";
        //}
        , editable: true
        , valueFormatter:  params => {
          return '$' + this.formatTruckLoad(params.data.truckLoad);
        }
      }
    ];
  }

  formatTruckLoad(number) {
    return Math.floor(number).toFixed(2).toString().replace(/(\d)(?=(\d{3})+(?!\d))/g, "$1,");
  }
  

  getAddedFreightDetails() {
    return this.http.get('./GetAddedFreightsDetails').subscribe(
      data => {
        this.addedFreightrowData = data;
      }
    )
  }

  AddAddedFreightRecord() {
    this.addedFreightGrid.api.updateRowData({
      add: [{ poLocationId: '', poWarehouseId: '', poCarrierId: '', vendorId: '', cwt: '', truckLoad: '' }],
      addIndex: 0
    });
    this.isNewRowAdded = true;
  }

  //DeleteAddedFreightRecord() {
  //  var selectedData = this.addedFreightGrid.api.getSelectedRows();
  //  this.addedFreightGrid.api.updateRowData({ remove: selectedData });
  //}

  onAddedFreightGridReady(params) {
    this.addedFreightgridApi = params.api;
    this.addedFreightgridColumnApi = params.columnApi;
  }

  onAddedFreightCellValueChanged(event) {
    event.data.modified = true;
    console.log(event)
  }

  SaveAddedFreightRecord() {
    if (this.isNewRowAdded) {
      const allRowData = [];
      this.addedFreightgridApi.forEachNode(node => allRowData.push(node.data));

      const modifiedRows = allRowData.filter(row => row['modified']);

      const formData = new FormData();

      formData.append('poLocationId', modifiedRows[0].poLocationId);
      formData.append('poWarehouseId', modifiedRows[0].poWarehouseId);
      formData.append('poCarrierId', modifiedRows[0].poCarrierId);
      formData.append('vendorId', modifiedRows[0].vendorId);
      formData.append('cwt', modifiedRows[0].cwt);
      formData.append('truckLoad', modifiedRows[0].truckLoad);

      // passing the params to server
      const uploadReq = new HttpRequest('POST', './PostAddedFreightsDetails', formData);
      this.toastr.info("Please wait while adding your data.", " Insertion in Progress...", { positionClass: 'toast-top-center', progressBar: false, progressAnimation: 'increasing' });

      this.http.request(uploadReq).subscribe(event => {
        console.log(event);
        if (event instanceof HttpResponse) {
          var response = event.body;

          if (response['Successful']) {
            this.toastr.clear();
            this.toastr.success(response['Message'], " Insertion Successful...", { positionClass: 'toast-top-center', timeOut: 3000, progressBar: false })
            this.reset();
          } else { //if there are errors   
            this.toastr.clear();
            this.toastr.error(response['Message'], "Insertion Failed...", { timeOut: 5000, progressBar: false });
          }
        }
      });
      this.isNewRowAdded = false;
    }
    else {
      
      if (this.addedFreightgridApi.getSelectedRows().length == 0) {
        this.toastr.error("error", "Please select Record for update");
        return;
      }
      var row = this.addedFreightgridApi.getSelectedRows();
      console.log(row);
      const formData = new FormData();
      formData.append('id', row[0].id);
      formData.append('poLocationId', row[0].poLocationId);
      formData.append('poWarehouseId', row[0].poWarehouseId);
      formData.append('poCarrierId', row[0].poCarrierId);
      formData.append('vendorId', row[0].vendorId);
      formData.append('cwt', row[0].cwt);
      formData.append('truckLoad', row[0].truckLoad);

      const req = new HttpRequest('POST', './UpdateAddedFreightDetails', formData);

      this.toastr.info("Please wait while updating your data.", " Updation in Progress...", { positionClass: 'toast-top-center', progressBar: false, progressAnimation: 'increasing' });

      this.http.request(req).subscribe(event => {
        console.log(event);
        if (event instanceof HttpResponse) {
          var response = event.body;

          if (response['Successful']) {
            this.toastr.clear();
            this.toastr.success(response['Message'], " Updation Successful...", { positionClass: 'toast-top-center', timeOut: 3000, progressBar: false })
            this.reset();
          } else { //if there are errors   
            this.toastr.clear();
            this.toastr.error(response['Message'], "Updation Failed...", { timeOut: 5000, progressBar: false });
          }
        }
      });
    }
    
  }

  DeleteAddedFreightRecord() {
    var selectedRow = this.addedFreightgridApi.getSelectedRows();
    if (selectedRow.length == 0) {
      this.toastr.error("error", "Please select a Record for deletion");
      return;
    }
    const formData = new FormData();
    formData.append('id', selectedRow[0].id);
    const req = new HttpRequest('POST', './DeleteAddedFreightRecord', formData);

    
    this.toastr.info("Please wait while removing your data.", " Deletion in Progress...", { positionClass: 'toast-top-center', progressBar: false, progressAnimation: 'increasing' });

    this.http.request(req).subscribe(event => {
      console.log(event);
      if (event instanceof HttpResponse) {
        var response = event.body;

        if (response['Successful']) {
          var selectedData = this.addedFreightGrid.api.getSelectedRows();
          this.addedFreightGrid.api.updateRowData({ remove: selectedData });
          this.toastr.clear();
          this.toastr.success(response['Message'], "Deletion Successful...", { positionClass: 'toast-top-center', timeOut: 3000, progressBar: false })
          this.reset();
        } else { //if there are errors   
          this.toastr.clear();
          this.toastr.error(response['Message'], "Deletion Failed...", { timeOut: 5000, progressBar: false });
        }
      }
    });
  }
  

  //Transfer Freight
  createTransferFreightcolumnDefs() {
    this.TransferFreightcolumnDefs = [
      {
        field: "transferFromId", headerName: "Transfer From", width: "110", editable: true, cellEditor: 'agSelectCellEditor',
        cellEditorParams: {
          values: this.extractValues(this.locations),
        }
        , refData: this.locations
        , required: true
      },
      {
        field: "transferToId", headerName: "Transfer To", width: "110", editable: true, cellEditor: 'agSelectCellEditor',
        cellEditorParams: {
          values: this.extractValues(this.locations),
        }
        , refData: this.locations
        , required: true
      },
      {
        field: "productCode", headerName: "Product Code", width: "120", editable: true, cellEditor: 'agSelectCellEditor',
        cellEditorParams: {
          values: this.extractValues(this.productCode),
        }
        , refData: this.productCode
        , required: true
      },//TODO: type ahead search
      {
        field: "transferCost", headerName: "Transfer Cost/CWT", width: "140", editable: true
        , valueFormatter: params => {
          return '$' + this.formatTruckLoad(params.data.transferCost);
        }
        , required: true
      }
    ];

  }
  getTransferFreightDetails() {
    return this.http.get('./GetTransferFreightsDetails').subscribe(
      data => {
        this.transferFreightrowData = data;

      }
    )
  }

  AddTransferFreightRecord() {
    this.transferFreightGrid.api.updateRowData({
      add: [{ transferFromId: '', transferToId: '', productCode: '', transferCost: '' }],
      addIndex: 0
    });
    this.isNewRowAdded = true;
  }

  DeleteTransferFreightRecord() {
    
    var selectedRow = this.transferFreightgridApi.getSelectedRows();
    if (selectedRow.length == 0) {
      this.toastr.error("error", "Please select a Record for deletion");
      return;
    }
    console.log('id' + selectedRow[0].id);
    const formData = new FormData();
    formData.append('id', selectedRow[0].id);
    const req = new HttpRequest('POST', './DeleteTransferFreightRecord', formData);
    //const req = new HttpRequest('DELETE', './DeleteTransferFreightRecord?id=' + selectedRow[0].id);
    
    this.toastr.info("Please wait while removing your data.", " Deletion in Progress...", { positionClass: 'toast-top-center', progressBar: false, progressAnimation: 'increasing' });

    this.http.request(req).subscribe(event => {
      console.log(event);
      if (event instanceof HttpResponse) {
        var response = event.body;

        if (response['Successful']) {
          var selectedData = this.transferFreightGrid.api.getSelectedRows();
          this.transferFreightGrid.api.updateRowData({ remove: selectedData });
          this.toastr.clear();
          this.toastr.success(response['Message'], "Deletion Successful...", { positionClass: 'toast-top-center', timeOut: 3000, progressBar: false })
          this.reset();
        } else { //if there are errors   
          this.toastr.clear();
          this.toastr.error(response['Message'], "Deletion Failed...", { timeOut: 5000, progressBar: false });
        }
      }
    });

  }

  onTransferFreightGridReady(params) {
    this.transferFreightgridApi = params.api;
    this.transferFreightgridColumnApi = params.columnApi;
  }

  onTransferFreightCellValueChanged(event) {
    event.data.modified = true;
  }

  SaveTransferFreightRecord() {
    if (this.isNewRowAdded) {
      const allRowData = [];
      this.transferFreightgridApi.forEachNode(node => allRowData.push(node.data));
      const modifiedRows = allRowData.filter(row => row['modified']);
      const formData = new FormData();

      formData.append('transferFromId', modifiedRows[0].transferFromId);
      formData.append('transferToId', modifiedRows[0].transferToId);
      formData.append('productCode', modifiedRows[0].productCode);
      formData.append('transferCost', modifiedRows[0].transferCost);

      // passing the params to server
      const uploadReq = new HttpRequest('POST', './PostTransferFreightsDetails', formData);
      this.toastr.info("Please wait while adding your data.", " Insertion in Progress...", { positionClass: 'toast-top-center', progressBar: false, progressAnimation: 'increasing' });

      this.http.request(uploadReq).subscribe(event => {
        console.log(event);
        if (event instanceof HttpResponse) {
          var response = event.body;

          if (response['Successful']) {
            this.toastr.clear();
            this.toastr.success(response['Message'], " Insertion Successful...", { positionClass: 'toast-top-center', timeOut: 3000, progressBar: false })
            this.reset();
          } else { //if there are errors   
            this.toastr.clear();
            this.toastr.error(response['Message'], "Insertion Failed...", { timeOut: 5000, progressBar: false });
          }
        }
      });
      this.isNewRowAdded = false;
    }
    else {
      
      if (this.transferFreightgridApi.getSelectedRows().length == 0) {
        this.toastr.error("error", "Please select Record for update");
        return;
      }
      var row = this.transferFreightgridApi.getSelectedRows();
      console.log(row);
      const formData = new FormData();
      formData.append('id', row[0].id);
      formData.append('transferFromId', row[0].transferFromId);
      formData.append('transferToId', row[0].transferToId);
      formData.append('productCode', row[0].productCode);
      formData.append('transferCost', row[0].transferCost);

      const req = new HttpRequest('POST', './UpdateTransferFreightDetails', formData);

      this.toastr.info("Please wait while updating your data.", " Updation in Progress...", { positionClass: 'toast-top-center', progressBar: false, progressAnimation: 'increasing' });

      this.http.request(req).subscribe(event => {
        console.log(event);
        if (event instanceof HttpResponse) {
          var response = event.body;

          if (response['Successful']) {
            this.toastr.clear();
            this.toastr.success(response['Message'], " Updation Successful...", { positionClass: 'toast-top-center', timeOut: 3000, progressBar: false })
            this.reset();
          } else { //if there are errors   
            this.toastr.clear();
            this.toastr.error(response['Message'], "Updation Failed...", { timeOut: 5000, progressBar: false });
          }
        }
      });
    }
  }


  //Class Code Management
  createClassCodeManagementcolumnDefs() {
    this.ClassCodeManagementcolumnDefs = [
      {
        field: "classCodeID", headerName: "Class Code", width: "160", editable: true, cellEditor: 'agSelectCellEditor',
        cellEditorParams: {
          values: this.extractValues(this.classCode),
        }
        , refData: this.classCode
        , required: true
      },
      {
        field: "productCodeId", headerName: "Product Code", width: "140", editable: true, cellEditor: 'agSelectCellEditor',
        cellEditorParams: {
          values: this.extractValues(this.productCode),
        }
        , refData: this.productCode
        , required: true
      },
      {
        field: "locationId", headerName: "Location", width: "140", editable: true, cellEditor: 'agSelectCellEditor',
        cellEditorParams: {
          values: this.extractValues(this.locations),
        }
        , refData: this.locations
        , required: true
        
      },
      {
        field: "active", headerName: "Active", width: "100", editable: true, cellEditor: 'agSelectCellEditor',
        cellEditorParams: {
          values: this.extractValues(this.active),
        }
        , refData: this.active
        ,required: true
      }
    ];
  }

  getClassCodeManagementDetails() {
    return this.http.get('./GetClassCodeManagementDetails').subscribe(
      data => {
        this.classCodeManagementrowData = data;

      }
    )
  }

  AddClassCodeMgtRecord() {
    this.classCodeManagementGrid.api.updateRowData({
      add: [{ classCodeID: '', productCodeId: '', locationId: '', active: '' }],
      addIndex: 0
    });
    this.isNewRowAdded = true;
  }

  DeleteClassCodeMgtRecord() {
    
    var selectedRow = this.classCodeManagementgridApi.getSelectedRows();
    if (selectedRow.length == 0) {
      this.toastr.error("error", "Please select a Record for deletion");
      return;
    }
    const formData = new FormData();
    formData.append('id', selectedRow[0].id);
    //const req = new HttpRequest('DELETE', './DeleteClassCodesRecord?id=' + selectedRow[0].id);
    const req = new HttpRequest('POST', './DeleteClassCodesRecord', formData);
    
    this.toastr.info("Please wait while removing your data.", " Deletion in Progress...", { positionClass: 'toast-top-center', progressBar: false, progressAnimation: 'increasing' });

    this.http.request(req).subscribe(event => {
      console.log(event);
      if (event instanceof HttpResponse) {
        var response = event.body;

        if (response['Successful']) {
          var selectedData = this.classCodeManagementGrid.api.getSelectedRows();
          this.classCodeManagementGrid.api.updateRowData({ remove: selectedData });
          this.toastr.clear();
          this.toastr.success(response['Message'], "Deletion Successful...", { positionClass: 'toast-top-center', timeOut: 3000, progressBar: false })
          this.reset();
        } else { //if there are errors   
          this.toastr.clear();
          this.toastr.error(response['Message'], "Deletion Failed...", { timeOut: 5000, progressBar: false });
        }
      }
    });

  }

  onClassCodeMgtGridReady(params) {
    this.classCodeManagementgridApi = params.api;
    this.classCodeManagementColumnApi = params.columnApi;
  }

  onClassCodeMgtCellValueChanged(event) {
    event.data.modified = true;
  }

  SaveClassCodeMgtRecord() {
    if (this.isNewRowAdded) {
      const allRowData = [];
      this.classCodeManagementgridApi.forEachNode(node => allRowData.push(node.data));

      const modifiedRows = allRowData.filter(row => row['modified']);

      const formData = new FormData();

      formData.append('classCodeID', modifiedRows[0].classCodeID);
      formData.append('productCodeId', modifiedRows[0].productCodeId);
      formData.append('locationId', modifiedRows[0].locationId);
      formData.append('active', modifiedRows[0].active);

      // passing the params to server
      const uploadReq = new HttpRequest('POST', './PostClassCodesDetails', formData);
      this.toastr.info("Please wait while adding your data.", " Insertion in Progress...", { positionClass: 'toast-top-center', progressBar: false, progressAnimation: 'increasing' });

      this.http.request(uploadReq).subscribe(event => {
        console.log(event);
        if (event instanceof HttpResponse) {
          var response = event.body;

          if (response['Successful']) {
            this.toastr.clear();
            this.toastr.success(response['Message'], " Insertion Successful...", { positionClass: 'toast-top-center', timeOut: 3000, progressBar: false })
            this.reset();
          } else { //if there are errors   
            this.toastr.clear();
            this.toastr.error(response['Message'], "Insertion Failed...", { timeOut: 5000, progressBar: false });
          }
        }
      });
      this.isNewRowAdded = false;
    }
    else {
      
      if (this.classCodeManagementgridApi.getSelectedRows().length == 0) {
        this.toastr.error("error", "Please select Record for update");
        return;
      }
      var row = this.classCodeManagementgridApi.getSelectedRows();
      console.log(row);
      const formData = new FormData();
      formData.append('id', row[0].id);
      formData.append('classCodeID', row[0].classCodeID);
      formData.append('productCodeId', row[0].productCodeId);
      formData.append('locationId', row[0].locationId);
      formData.append('active', row[0].active);

      const req = new HttpRequest('POST', './UpdateClassCodeDetails', formData);

      this.toastr.info("Please wait while updating your data.", " Updation in Progress...", { positionClass: 'toast-top-center', progressBar: false, progressAnimation: 'increasing' });

      this.http.request(req).subscribe(event => {
        console.log(event);
        if (event instanceof HttpResponse) {
          var response = event.body;

          if (response['Successful']) {
            this.toastr.clear();
            this.toastr.success(response['Message'], " Updation Successful...", { positionClass: 'toast-top-center', timeOut: 3000, progressBar: false })
            this.reset();
          } else { //if there are errors   
            this.toastr.clear();
            this.toastr.error(response['Message'], "Updation Failed...", { timeOut: 5000, progressBar: false });
          }
        }
      });
    }
  }

  //Display Months
  createDisplayMonthscolumnDefs() {
    this.DisplayMonthscolumnDefs = [
      {
        field: "month", headerName: "Month", width: "150", editable: true, cellEditor: 'agSelectCellEditor',
        cellEditorParams: {
          values: this.extractValues(this.month),
        }
        , refData: this.month
        , required: true
      },
      {
        field: "year", headerName: "Year", width: "150", editable: true, cellEditor: 'agSelectCellEditor',
        cellEditorParams: {
          values: ['2022', '2023', '2024', '2025', '2026', '2027', '2028', '2029', '2030',
            '2031', '2032', '2033', '2034', '2035', '2036', '2037', '2038', '2039', '2040',
            '2041', '2042', '2043', '2044', '2045', '2046', '2047', '2048', '2049', '2050'
          ]
        }
        , required: true
      },
      {
        field: "active", headerName: "Active", width: "120", editable: true, cellEditor: 'agSelectCellEditor',
        cellEditorParams: {
          values: this.extractValues(this.active),
        }
        , refData: this.active
        , required: true
      }
    ];
  }

  getDisplayMonthsDetails() {
    return this.http.get('./GetDisplayMonthsDetails').subscribe(
      data => {
        this.displayMonthsrowData = data;

      }
    )
  }

  AddDisplayMonthsRecord() {
    this.displayMonthsGrid.api.updateRowData({
      add: [{ month: '', year: '', active: '' }],
      addIndex: 0
    });
    this.isNewRowAdded = true;
  }

  DeleteDisplayMonthsRecord() {
    
    var selectedRow = this.displayMonthsgridApi.getSelectedRows();
    if (selectedRow.length == 0) {
      this.toastr.error("error", "Please select a Record for deletion");
      return;
    }
    const formData = new FormData();
    formData.append('id', selectedRow[0].id);
    //const req = new HttpRequest('DELETE', './DeleteDisplayMonthsRecord?id=' + selectedRow[0].id);
    const req = new HttpRequest('POST', './DeleteDisplayMonthsRecord', formData);
    
    this.toastr.info("Please wait while removing your data.", " Deletion in Progress...", { positionClass: 'toast-top-center', progressBar: false, progressAnimation: 'increasing' });

    this.http.request(req).subscribe(event => {
      console.log(event);
      if (event instanceof HttpResponse) {
        var response = event.body;

        if (response['Successful']) {
          var selectedData = this.displayMonthsGrid.api.getSelectedRows();
          this.displayMonthsGrid.api.updateRowData({ remove: selectedData });
          this.toastr.clear();
          this.toastr.success(response['Message'], "Deletion Successful...", { positionClass: 'toast-top-center', timeOut: 3000, progressBar: false })
          this.reset();
        } else { //if there are errors   
          this.toastr.clear();
          this.toastr.error(response['Message'], "Deletion Failed...", { timeOut: 5000, progressBar: false });
        }
      }
    });
  }

  onDisplayMonthsGridReady(params) {
    this.displayMonthsgridApi = params.api;
    this.displayMonthsColumnApi = params.columnApi;
  }

  onDisplayMonthCellValueChanged(event) {
    event.data.modified = true;
  }

  SaveDisplayMonthsRecord() {
    if (this.isNewRowAdded) {
      const allRowData = [];
      this.displayMonthsgridApi.forEachNode(node => allRowData.push(node.data));

      const modifiedRows = allRowData.filter(row => row['modified']);

      const formData = new FormData();

      formData.append('month', modifiedRows[0].month);
      formData.append('year', modifiedRows[0].year);
      formData.append('active', modifiedRows[0].active);

      // passing the params to server
      const uploadReq = new HttpRequest('POST', './PostDisplayMonthsDetails', formData);
      this.toastr.info("Please wait while adding your data.", " Insertion in Progress...", { positionClass: 'toast-top-center', progressBar: false, progressAnimation: 'increasing' });

      this.http.request(uploadReq).subscribe(event => {
        console.log(event);
        if (event instanceof HttpResponse) {
          var response = event.body;

          if (response['Successful']) {
            this.toastr.clear();
            this.toastr.success(response['Message'], " Insertion Successful...", { positionClass: 'toast-top-center', timeOut: 3000, progressBar: false })
            this.reset();
          } else { //if there are errors   
            this.toastr.clear();
            this.toastr.error(response['Message'], "Insertion Failed...", { timeOut: 5000, progressBar: false });
          }
        }
      });
      this.isNewRowAdded = false;
    }
    else {
      
      if (this.displayMonthsgridApi.getSelectedRows().length == 0) {
        this.toastr.error("error", "Please select Display Month Record for update");
        return;
      }
      var row = this.displayMonthsgridApi.getSelectedRows();
      console.log(row);
      const formData = new FormData();
      formData.append('id', row[0].id);
      formData.append('month', row[0].month);
      formData.append('year', row[0].year);
      formData.append('active', row[0].active);

      const req = new HttpRequest('POST', './UpdateDisplayMonthsDetails', formData);
      var selectedData = this.displayMonthsGrid.api.getSelectedRows();
      this.displayMonthsGrid.api.updateRowData({ update: selectedData });
      this.toastr.info("Please wait while updating your data.", " Updation in Progress...", { positionClass: 'toast-top-center', progressBar: false, progressAnimation: 'increasing' });

      this.http.request(req).subscribe(event => {
        console.log(event);
        if (event instanceof HttpResponse) {
          var response = event.body;

          if (response['Successful']) {
            this.toastr.clear();
            this.toastr.success(response['Message'], " Updation Successful...", { positionClass: 'toast-top-center', timeOut: 3000, progressBar: false })
            this.reset();
          } else { //if there are errors   
            this.toastr.clear();
            this.toastr.error(response['Message'], "Updation Failed...", { timeOut: 5000, progressBar: false });
          }
        }
      });
    }
  }

  openDatePicker() {
    //this.displayDatePicker = true;
    this.displayDatePicker = true;
    this.InputVar.nativeElement.value = "";
    this.data = [[], []];
    this.header = [[], []];
  }

  reset() {
    // We will clear the value of the input 
    // field using the reference variable.
    document.getElementById("btnUpload").blur();
    document.getElementById("btnReset").blur();
    
    this.displayDatePicker = false;
    this.displayGrid = false;
    this.displayerrors = false;
    this.InputVar.nativeElement.value = "";
    this.data = [[], []];
    this.header = [[], []];

    if(this.typeOfFile == 'F_02'){
      this.openDatePicker();
    }
  }

  errorDataOrTableData() {
    if (this.errorlist.length) {
      this.InputVar.nativeElement.value = "";
      this.displayerrors = true;
      this.displayGrid = false;

    } 
  }

 
  upload() {
    var files = this.selectedFile;
    if (this.typeOfFile == 'F_04') {
      return;
    }

    //check if the file is selected or not.
    if (files.length === 0) {
      this.toastr.error('Please select a file to upload.');
      return;
    }
    //check if the file type is CRU, then it must have a datepicker.
    if (this.typeOfFile == 'F_02' && (this.to == null || this.from == null)) {
      this.toastr.error('Please select FROM and TO date');
      document.getElementById("fromdate").focus();
      document.getElementById("todate").focus();
      return;
    }
    //converted dates into number to compare which is greater.
    if (this.typeOfFile == 'F_02') {
      var from = new Date(this.from)
      var start = from.getFullYear();
      var to = new Date(this.to)
      var end = to.getFullYear();
      console.log(start);
      if (start>end) {
        this.toastr.error('Start Date cannot be greater than end Date');
        document.getElementById("fromdate").focus();
        document.getElementById("todate").focus();
        return;
      }
    }
    const formData = new FormData();

    for (const file of files) {
      formData.append(file.name, file);
    }
    // passing the params to server
    const uploadReq = new HttpRequest('POST', this.baseUrl + 'FileUpload/upload', formData, {
      reportProgress: true,
      params: new HttpParams().set('typeOfFile', this.typeOfFile).set('from', this.from).set('to',this.to)
    });
    //check
    
    this.toastr.info("Please wait while your file is being uploaded.", " Upload in Progress...", { positionClass: 'toast-top-center', progressBar: false, progressAnimation: 'increasing' });
    this.http.request(uploadReq).subscribe(event => {
      
      if (event instanceof HttpResponse) {
        var response = event.body;
        this.errorlist = response['ErrorList'];
        this.errorDataOrTableData()   // called to remove the table grid and put the error grid
       

        if (response['Successful']) {
          this.toastr.clear(); //to clear the old toaster of file upload in progress
          this.toastr.success("Your file has been uploaded successfully.", " Upload Successful...", { positionClass: 'toast-top-center', timeOut: 3000, progressBar: false })
          this.reset();
        } else { //if there are errors in the file         
          this.toastr.clear();
            this.toastr.error("There are some errors in the excel file.","Upload Failed...", { timeOut: 5000, progressBar: false });
         
        }
      }
    });
    document.getElementById("btnUpload").blur();
    document.getElementById("btnReset").blur();
  }

  onFileChange(evt: any, file) {
    console.log('onFileChange selected')
    this.displayGrid = true;
    this.displayerrors = false;
    const target: DataTransfer = <DataTransfer>(evt.target);
    if (target.files.length == 0) {
      this.displayGrid = false;
    }
    if (target.files.length > 1) {
      this.toastr.error('Cannot upload multiple files');
    }

    this.selectedFile = target.files;
    console.log(this.selectedFile);
    this.fileName = target.files[0].name != null ? target.files[0].name : "";
    let allowedExtensions = /(\.xls|\.xlsx)$/i;  // to allow only excel files
    if (!allowedExtensions.exec(this.fileName)) {
      this.toastr.error('Please select an Excel File');
      file.value = '';
    }
    const reader: FileReader = new FileReader();

    reader.onload = (e: any) => {
      let isValidFile: boolean

      const bstr: string = e.target.result;

      const wb: XLSX.WorkBook = XLSX.read(bstr, { type: 'binary' });

      const wsname: string = wb.SheetNames[0];

      const ws: XLSX.WorkSheet = wb.Sheets[wsname];

      this.sheet = (XLSX.utils.sheet_to_json(ws, { header: 1, raw: false }));
      var headerEnd = this.typeOfFile == GlobalConstants.F_02 ? 8 : 1
      // seperating the header and the actual data
      this.data = this.sheet.slice(headerEnd);
      this.header = this.sheet.slice(0, headerEnd);
      
      isValidFile = this.checkFileValidation();
      if (!isValidFile) {
        this.toastr.error('Columns do not match. Please select appropriate file for File Type');
        this.reset();
      }
    };

    reader.readAsBinaryString(target.files[0]);

  }

  checkFileValidation(): boolean {
    let isValidFile: boolean
    //the '' column is to differentiate the salesforecast and the plannedbuy
    let fileColumnHeader: string[][] = [['Year', 'Month', 'Location', 'Amount'], ['Spot prices', 'WEEK 1', 'WEEK 2', 'WEEK 3', 'WEEK 4', 'WEEK 5'], ['Year', 'Month', 'Location', 'Amount','CWT']];

    if (this.typeOfFile == GlobalConstants.F_01) { // no role of the 5th column in salesforecast.
      if (this.header[0].length.valueOf() == 4) {
        for (var i = 0; i < 4; i++) {
          if (this.header[0][i] == fileColumnHeader[0][i])
            isValidFile = true;
          else
            isValidFile = false
        }
      }
      else {
        isValidFile = false;
      }
      
    }
    else if (this.typeOfFile == GlobalConstants.F_02) {
      if (this.header[0].length.valueOf() == 6) {
        for (var i = 0; i < 6; i++) {
          if (this.header[0][i] == fileColumnHeader[1][i])
            isValidFile = true;
          else
            isValidFile = false
          }
        }
      else {
        isValidFile = false;
      }
    }
      else if (this.typeOfFile == GlobalConstants.F_03) {
        if (this.header[0].length.valueOf() == 5) {
        for (var i = 0; i < 5; i++) {
          if (this.header[0][i] == fileColumnHeader[2][i])
            isValidFile = true;
          else
            isValidFile = false
            }
          }
        else {
          isValidFile = false;
        }
    }
    return isValidFile;
  }
}
