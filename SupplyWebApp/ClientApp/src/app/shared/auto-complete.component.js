"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.AutoCompleteComponent = void 0;
var core_1 = require("@angular/core");
var AutoCompleteComponent = /** @class */ (function () {
    function AutoCompleteComponent(httpClient) {
        this.httpClient = httpClient;
        this.rowSelection = 'single';
        this.gridHeight = 175;
        this.gridWidth = 375;
        this.isCanceled = true;
        this.selectedObject = {};
    }
    AutoCompleteComponent.prototype.ngAfterViewInit = function () {
        var _this = this;
        window.setTimeout(function () {
            if (_this.inputValue == _this.cellValue) {
                _this.input.nativeElement.select();
            }
            else {
                _this.input.nativeElement.focus();
            }
            if (_this.inputValue && !_this.useApi)
                _this.updateFilter();
        });
    };
    // ICellEditorAngularComp functions
    AutoCompleteComponent.prototype.agInit = function (params) {
        this.params = params;
        if (!params.rowData) {
            this.apiEndpoint = params.apiEndpoint;
            this.useApi = true;
            this.rowData = [{}];
        }
        else {
            this.rowData = params.rowData;
        }
        if (params.gridHeight)
            this.gridHeight = params.gridHeight;
        if (params.gridWidth)
            this.gridWidth = params.gridWidth;
        this.columnDefs = params.columnDefs;
        this.propertyName = params.propertyRendered;
        this.cellValue = params.value[this.propertyName];
        this.returnObject = params.returnObject;
        if (!params.charPress) {
            if (this.cellValue)
                this.inputValue = this.cellValue;
        }
        else {
            this.inputValue = params.charPress;
        }
    };
    AutoCompleteComponent.prototype.getValue = function () {
        if (!this.returnObject)
            return this.selectedObject[this.propertyName];
        return this.selectedObject;
    };
    AutoCompleteComponent.prototype.isPopup = function () {
        return true;
    };
    AutoCompleteComponent.prototype.isCancelAfterEnd = function () {
        return this.isCanceled;
    };
    // ag-Grid functions
    AutoCompleteComponent.prototype.onGridReady = function (params) {
        this.gridApi = params.api;
        this.gridApi.sizeColumnsToFit();
        this.columnFilter = this.gridApi.getFilterInstance(this.propertyName);
    };
    // component functions
    AutoCompleteComponent.prototype.rowClicked = function (params) {
        this.selectedObject = params.data;
        this.isCanceled = false;
        this.params.api.stopEditing();
    };
    AutoCompleteComponent.prototype.rowConfirmed = function () {
        if (this.gridApi.getSelectedRows()[0]) {
            this.selectedObject = this.gridApi.getSelectedRows()[0];
            this.isCanceled = false;
        }
        this.params.api.stopEditing();
    };
    AutoCompleteComponent.prototype.onKeydown = function (event) {
        event.stopPropagation();
        if (event.key == "Escape") {
            this.params.api.stopEditing();
            return false;
        }
        if (event.key == "Enter" || event.key == "Tab") {
            this.rowConfirmed();
            return false;
        }
        if (event.key == "ArrowUp" || event.key == "ArrowDown") {
            this.navigateGrid();
            return false;
        }
    };
    AutoCompleteComponent.prototype.processDataInput = function (event) {
        var _this = this;
        if (this.useApi) {
            if (event.length == 0)
                this.gridApi.setRowData();
            if (event.length == 2) {
                this.getApiData(event).subscribe(function (data) {
                    _this.rowData = data;
                    setTimeout(function () { _this.updateFilter(); });
                });
            }
            ;
            if (event.length > 2)
                this.updateFilter();
        }
        else {
            this.updateFilter();
        }
    };
    AutoCompleteComponent.prototype.getApiData = function (filter) {
        return this.httpClient.get(this.apiEndpoint + this.toQueryString(filter));
    };
    AutoCompleteComponent.prototype.toQueryString = function (filter) {
        return "?" + this.propertyName + "=" + filter;
    };
    AutoCompleteComponent.prototype.updateFilter = function () {
        this.columnFilter.setModel({
            type: 'startsWith',
            filter: this.inputValue,
        });
        this.columnFilter.onFilterChanged();
        if (this.gridApi.getDisplayedRowAtIndex(0)) {
            this.gridApi.getDisplayedRowAtIndex(0).setSelected(true);
            this.gridApi.ensureIndexVisible(0, 'top');
        }
        else {
            this.gridApi.deselectAll();
        }
    };
    AutoCompleteComponent.prototype.navigateGrid = function () {
        if (this.gridApi.getFocusedCell() == null || this.gridApi.getDisplayedRowAtIndex(this.gridApi.getFocusedCell().rowIndex) == null) { // check if no cell has focus, or if focused cell is filtered
            this.gridApi.setFocusedCell(this.gridApi.getDisplayedRowAtIndex(0).rowIndex, this.propertyName);
            this.gridApi.getDisplayedRowAtIndex(this.gridApi.getFocusedCell().rowIndex).setSelected(true);
        }
        else {
            this.gridApi.setFocusedCell(this.gridApi.getFocusedCell().rowIndex, this.propertyName);
            this.gridApi.getDisplayedRowAtIndex(this.gridApi.getFocusedCell().rowIndex).setSelected(true);
        }
    };
    __decorate([
        core_1.ViewChild('input', { static: false })
    ], AutoCompleteComponent.prototype, "input", void 0);
    __decorate([
        core_1.HostListener('keydown', ['$event'])
    ], AutoCompleteComponent.prototype, "onKeydown", null);
    AutoCompleteComponent = __decorate([
        core_1.Component({
            selector: 'auto-complete',
            encapsulation: core_1.ViewEncapsulation.None,
            host: {
                style: "position: absolute;\n\t\t\t\t\tleft: 0px; \n\t\t\t\t\ttop: 0px;\n\t\t\t\t\tbackground-color: black;\n\t\t\t\t\t"
            },
            template: " \n\t\t<input #input\n\t\t\t[(ngModel)]=\"inputValue\"\n\t\t\t(ngModelChange)=\"processDataInput($event)\"\n\t\t\tstyle=\" height: 28px; font-weight: 400; font-size: 12px;\"\n\t\t\t[style.width]=\"params.column.actualWidth + 'px'\">\n\t\t<ag-grid-angular\n\t\t\tstyle=\"font-weight: 150;\" \n\t\t\t[style.height]=\"gridHeight + 'px'\"\n\t\t\t[style.max-width]=\"gridWidth + 'px'\"\n\t\t\tclass=\"ag-theme-balham\"\n\t\t\t[rowData]=\"rowData\" \n\t\t\t[columnDefs]=\"columnDefs\"\n\t\t\t[rowSelection]=\"rowSelection\"\n\t\t\t(gridReady)=\"onGridReady($event)\"\n\t\t\t(rowClicked)=\"rowClicked($event)\">\n\t\t</ag-grid-angular>\n\t"
        })
    ], AutoCompleteComponent);
    return AutoCompleteComponent;
}());
exports.AutoCompleteComponent = AutoCompleteComponent;
//# sourceMappingURL=auto-complete.component.js.map