import { HttpClient, HttpRequest, HttpEventType, HttpResponse, HttpHeaders, HttpParams } from '@angular/common/http'
import { Component, ElementRef, Inject, Input, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import * as XLSX from 'xlsx';
import { Toast, ToastrService } from 'ngx-toastr';
import { GlobalConstants } from '../common/global-constant';
import { ActivatedRoute } from '@angular/router';
import { AddedFreight } from '../model/AddedFreight';
import { UploadService } from '../service/upload.service';
import { AgGridAngular } from 'ag-grid-angular';
import { GridOptions } from 'ag-grid-community';
import { Observable } from 'rxjs';

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

  //Margin Tables constants
  addedFreightrowData: any;
  transferFreightrowData: any;
  classCodeManagementrowData: any;
  displayMonthsrowData: any;
  @ViewChild('addedFreightGrid', { static: false }) addedFreightGrid: AgGridAngular;
  @ViewChild('transferFreightGrid', { static: false }) transferFreightGrid: AgGridAngular;
  @ViewChild('classCodeManagementGrid', { static: false }) classCodeManagementGrid: AgGridAngular;
  @ViewChild('displayMonthsGrid', { static: false }) displayMonthsGrid: AgGridAngular;
  private gridApi;
  private gridColumnApi;

  AddedFreightcolumnDefs = [
    {
      field: "poLocationId", headerName: "PO Location", width: "90", editable: true, cellEditor: 'agSelectCellEditor',
      cellEditorParams: { values: ['IND', 'PIT', 'DEN', 'FTW'] }, required: true
    },
    {
      field: "poWarehouseId", headerName: "PO Warehouse", width: "110", editable: true, cellEditor: 'agSelectCellEditor',
      cellEditorParams: { values: ['IND', 'PIT', 'DEN', 'FTW'] }, required: true//TODO: values from Warehouse.Abb
    },
    {
      field: "poCarrierId", headerName: "PO Carrier", width: "90", editable: true, cellEditor: 'agSelectCellEditor',
      cellEditorParams: { values: ['Will Call', 'Delivery'] }
    },
    {
      field: "vendorId", headerName: "Vendor", width: "90", editable: true, cellEditor: 'agSelectCellEditor',
      cellEditorParams: { values: ['IND', 'PIT', 'DEN', 'FTW'] }, required: true //TODO:type ahead search + 'CheckName’ values in the Vendor db table.
    },
    { field: "cwt", headerName: "\"Added Freight/CWT\"", width: "150", editable: true, required: true },
    { field: "truckLoad", headerName: "$/Truckload", width: "100", editable: true, required: true  }
  ];

  TransferFreightcolumnDefs = [
    {
      field: "transferfromId", headerName: "Transfer From", width: "120", editable: true, cellEditor: 'agSelectCellEditor',
      cellEditorParams: { values: ['IND', 'PIT', 'DEN', 'FTW'] }, required: true
    },
    {
      field: "transfertoId", headerName: "Transfer To", width: "120", editable: true, cellEditor: 'agSelectCellEditor',
      cellEditorParams: { values: ['IND', 'PIT', 'DEN', 'FTW'] }, required: true
    },
    { field: "productCode", headerName: "Product Code", width: "120", editable: true, required: true },//TODO: type ahead search + PartNo column from the Part table.
    { field: "transferCost", headerName: "Transfer Cost/CWT", width: "140", editable: true, required: true  }
  ];

  ClassCodeManagementcolumnDefs = [
    { field: "classCodeID", headerName: "Class Code", width: "160", editable: true, required: true },//TODO: ‘Code’ column from the ‘ClassCode’ table
    { field: "productcodeId", headerName: "Product Code", width: "140", editable: true, required: true },//TODO: PartNo column from the Part table.
    {
      field: "locationId", headerName: "Location", width: "140", editable: true, cellEditor: 'agSelectCellEditor',
      cellEditorParams: { values: ['IND', 'PIT', 'DEN', 'FTW'] }, required: true
    },
    {
      field: "active", headerName: "Active", width: "140", editable: true, cellEditor: 'agSelectCellEditor',
      cellEditorParams: { values: ['True', 'False'] }, required: true
    }
  ];

  DisplayMonthscolumnDefs = [
    { field: "month", headerName: "Month", width: "150", editable: true, cellEditor: 'agSelectCellEditor',
    cellEditorParams: { values: ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'] }, required: true },
    {
      field: "year", headerName: "Year", width: "150", editable: true, cellEditor: 'agSelectCellEditor',
      cellEditorParams: {
        values: ['2022', '2023', '2024', '2025', '2026', '2027', '2028', '2029', '2030',
          '2031', '2032', '2033', '2034', '2035', '2036', '2037', '2038', '2039', '2040',
          '2041', '2042', '2043', '2044', '2045', '2046', '2047', '2048', '2049', '2050'
        ]
      }, required: true
    },
    {
      field: "active", headerName: "Active", width: "120", editable: true, cellEditor: 'agSelectCellEditor',
      cellEditorParams: { values: ['True', 'False'] }, required: true }
  ];

  constructor(private http: HttpClient, @Inject('BASE_URL') baseUrl: string, private toastr: ToastrService, private route: ActivatedRoute, private uploadService: UploadService) {
    this.baseUrl = baseUrl;
    
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

  //Added Freight
  getAddedFreightDetails() {
    return this.http.get('https://localhost:44341/GetAddedFreightsDetails').subscribe(
      data => {
        this.addedFreightrowData = data;

      }
    )
  }

  AddAddedFreightRecord() {
    this.addedFreightGrid.api.updateRowData({
      add: [{ pO_LocationId: '', pO_WarehouseId: '', pO_CarrierId: '', vendorId: '', cwt: '', truckLoad:''}]
    });
  }

  DeleteAddedFreightRecord() {
    var selectedData = this.addedFreightGrid.api.getSelectedRows();
    this.addedFreightGrid.api.updateRowData({ remove: selectedData });
  }

  onGridReady(params) {
    this.gridApi = params.api;
    this.gridColumnApi = params.columnApi;
  }

  onCellValueChanged(event) {
    event.data.modified = true;
  }

  SaveAddedFreightRecord() {
    //const allRowData = [];
    let input= [{ pO_LocationId: 'IND', pO_WarehouseId: 'IND', pO_CarrierId: 'IND', vendorId: 'CSI', cwt: '$0.98', truckLoad: '$245.00' }]
    //this.gridApi.forEachNode(node => allRowData.push(node.data));

    //use below if we want to save only modified rows
    //const modifiedRows = allRowData.filter(row => row['modified']);

  
    //this.uploadService.PostAddedFreightsDetails(input).subscribe(
    //  data => {
    //    console.log(data);
    //  }
    //);
    let url = 'https://localhost:44341/PostAddedFreightsDetails'
    this.http.post(url, { poLocationId: 'IND', poWarehouseId: 'IND', poCarrierId: 'IND', vendorId: 'CSI', cwt: '$0.98', truckLoad: '$245.00' }
    ).toPromise().then((data: any) => {
      console.log(data)
    })

  }

  

  //Transfer Freight
  getTransferFreightDetails() {
    return this.http.get('https://localhost:44341/GetTransferFreightsDetails').subscribe(
      data => {
        this.transferFreightrowData = data;

      }
    )
  }

  AddTransferFreightRecord() {
    this.transferFreightGrid.api.updateRowData({
      add: [{ transfer_from_Id: '', transfer_to_Id: '', product_Code: '', transfer_Cost: '' }]
    });
  }

  DeleteTransferFreightRecord() {
    var selectedData = this.transferFreightGrid.api.getSelectedRows();
    this.transferFreightGrid.api.updateRowData({ remove: selectedData });
  }


  //Class Code Management
  getClassCodeManagementDetails() {
    return this.http.get('https://localhost:44341/GetClassCodeManagementDetails').subscribe(
      data => {
        this.classCodeManagementrowData = data;

      }
    )
  }

  AddClassCodeMgtRecord() {
    this.classCodeManagementGrid.api.updateRowData({
      add: [{ class_CodeID: '', product_codeId: '', locationId: '', active: '' }]
    });
  }

  DeleteClassCodeMgtRecord() {
    var selectedData = this.classCodeManagementGrid.api.getSelectedRows();
    this.classCodeManagementGrid.api.updateRowData({ remove: selectedData });
  }


  //Display Months
  getDisplayMonthsDetails() {
    return this.http.get('https://localhost:44341/GetDisplayMonthsDetails').subscribe(
      data => {
        this.displayMonthsrowData = data;

      }
    )
  }

  AddDisplayMonthsRecord() {
    this.displayMonthsGrid.api.updateRowData({
      add: [{ month: '', year: '', active: ''}]
    });
  }

  DeleteDisplayMonthsRecord() {
    var selectedData = this.displayMonthsGrid.api.getSelectedRows();
    this.displayMonthsGrid.api.updateRowData({ remove: selectedData });
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

 
  upload(files) {

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
    let fileColumnHeader: string[][] = [['Year', 'Month', 'Location', 'Amount',''], ['Spot prices', 'WEEK 1', 'WEEK 2', 'WEEK 3', 'WEEK 4', 'WEEK 5'], ['Year', 'Month', 'Location', 'Amount','CWT']];

    if (this.typeOfFile == GlobalConstants.F_01) { // no role of the 5th column in salesforecast.
      for (var i = 0; i < 4; i++) {
        if (this.header[0][i] == fileColumnHeader[0][i])
          isValidFile = true;
        else
          isValidFile = false
      }
    }
    else if (this.typeOfFile == GlobalConstants.F_02) {
      for (var i = 0; i < 5; i++) {
        if (this.header[0][i] == fileColumnHeader[1][i])
          isValidFile = true;
        else
          isValidFile = false
      }
    }
    else if (this.typeOfFile == GlobalConstants.F_03) {
      for (var i = 0; i < 5; i++) {
        if (this.header[0][i] == fileColumnHeader[2][i])
          isValidFile = true;
        else
          isValidFile = false
      }
    }
    return isValidFile;
  }
}
