USE [Cemco_DW]
GO
/****** Object:  StoredProcedure [Qlik].[Snapshot]    Script Date: 20-07-2022 13:07:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Jason Wu
-- Create date: 2022-6-8
-- Description:	Sproc for comparing planned and actual shipments
-- EXEC [Qlik].[Snapshot]
-- =============================================
ALTER PROCEDURE [Qlik].[Snapshot]

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	SET ARITHABORT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;


	-- No parameter sniffing
	DECLARE @_DaysBack			INT			= 0;


	-- Generate "schema.procedure" name
	DECLARE @_StoredProcedure NVARCHAR(128) = OBJECT_SCHEMA_NAME(@@PROCID) + CONVERT(NCHAR(1), '.') + OBJECT_NAME(@@PROCID);

	-- Generate XML from current parameter list
	DECLARE @_Parameter XML = (
		SELECT
			@_DaysBack			AS DaysBack
			FOR XML PATH('root'), TYPE
	);

	-- Create log entry
	EXEC Administration.StoredProcedure_CreateExecutionLog
		@StoredProcedure	= @_StoredProcedure,
		@Parameter			= @_Parameter;



	BEGIN TRAN

		--Get the last working date
		DECLARE @_CompareDate DATE = (SELECT
										TOP 1 DATE

										FROM Reporting.Date
										WHERE DATE < CONVERT(DATE, GETDATE())
											AND DATE NOT IN (SELECT CONVERT(DATE, NonWorkingDay) FROM Reporting.NonWorkingDay)
										ORDER BY Date DESC);

		--DECLARE @_CompareDate DATE = DATEADD(DAY, 0, GETDATE());



		--Sales Order Shipped Early
		IF OBJECT_ID('tempdb..#Early') IS NOT NULL
			DROP TABLE #Early;

		SELECT
			'Sales Order Shipped Early'											AS 'SnapshotCategory',
			s.Snapshot_SalesOrderDate											AS 'SnapshotPlannedShipDate',
			s.SalesOrderID														AS 'SnapshotSalesOrderID',
			s.SalesOrderItemID													AS 'SnapshotSalesOrderItemID',
			s.SalesOrderLineItemNumber											AS 'SnapshotSalesOrderLineItemNumber',
			s.SalesOrderOrderDate												AS 'SnapshotSalesOrderDate',
			s.SalesOrderOrderLocationID,
			s.SalesOrderCustomerID,
			s.SalesOrderPartDetailID,
			s.SalesOrderWeight													AS 'SnapshotPlannedWeight',
			s.SalesOrderExtendedPrice + COALESCE(s.SalesOrderChargePrice, 0)	AS 'SnapshotPlannedAmount',

			i.InvoiceID															AS 'SnapshotInvoiceID',
			i.InvoiceItemID														AS 'SnapshotInvoiceItemID',
			i.InvoiceRowNum														AS 'SnapshotInvoiceLineItemNumber',
			i.InvoiceDate														AS 'SnapshotActualShipDate',
			i.InvoiceWeight														AS 'SnapshotActualWeight',
			i.InvoiceExtendedPrice + COALESCE(i.InvoiceChargePrice, 0)
			+COALESCE(i.InvoiceTaxCollectable, 0)								AS 'SnapshotActualAmount'

			INTO #Early
			FROM Reporting.Snapshot_SalesOrder AS s

				JOIN Summary.Invoice AS i
					ON i.SalesOrderID = s.SalesOrderID
						AND i.SalesOrderItemID = s.SalesOrderItemID

			WHERE	s.Snapshot_SalesOrderDate = @_CompareDate
				AND i.InvoiceDate < @_CompareDate
				AND i.InvoiceStatus IN (2,4,5);



		--Sales Order Shipped AS Planned
		IF OBJECT_ID('tempdb..#AsPlanned') IS NOT NULL
			DROP TABLE #AsPlanned;

		SELECT
			'Sales Order Shipped As Planned'									AS 'SnapshotCategory',
			s.Snapshot_SalesOrderDate											AS 'SnapshotPlannedShipDate',
			s.SalesOrderID														AS 'SnapshotSalesOrderID',
			s.SalesOrderItemID													AS 'SnapshotSalesOrderItemID',
			s.SalesOrderLineItemNumber											AS 'SnapshotSalesOrderLineItemNumber',
			s.SalesOrderOrderDate												AS 'SnapshotSalesOrderDate',
			s.SalesOrderOrderLocationID,
			s.SalesOrderCustomerID,
			s.SalesOrderPartDetailID,
			s.SalesOrderWeight													AS 'SnapshotPlannedWeight',
			s.SalesOrderExtendedPrice + COALESCE(s.SalesOrderChargePrice, 0)	AS 'SnapshotPlannedAmount',

			i.InvoiceID															AS 'SnapshotInvoiceID',
			i.InvoiceItemID														AS 'SnapshotInvoiceItemID',
			i.InvoiceRowNum														AS 'SnapshotInvoiceLineItemNumber',
			i.InvoiceDate														AS 'SnapshotActualShipDate',
			i.InvoiceWeight														AS 'SnapshotActualWeight',
			i.InvoiceExtendedPrice + COALESCE(i.InvoiceChargePrice, 0)
			+COALESCE(i.InvoiceTaxCollectable, 0)								AS 'SnapshotActualAmount'

			INTO #AsPlanned
			FROM Reporting.Snapshot_SalesOrder AS s

				LEFT JOIN Summary.Invoice AS i
					ON i.SalesOrderID = s.SalesOrderID
						AND i.SalesOrderItemID = s.SalesOrderItemID

			WHERE	s.Snapshot_SalesOrderDate = @_CompareDate
				AND  i.InvoiceDate = @_CompareDate
				AND i.InvoiceStatus IN (2,4,5);



		--Sales Order Didn’t Ship So Far
		IF OBJECT_ID('tempdb..#NoShip') IS NOT NULL
			DROP TABLE #NoShip;

		SELECT
			'Sales Order Did Not Ship'											AS 'SnapshotCategory',
			s.Snapshot_SalesOrderDate											AS 'SnapshotPlannedShipDate',
			s.SalesOrderID														AS 'SnapshotSalesOrderID',
			s.SalesOrderItemID													AS 'SnapshotSalesOrderItemID',
			s.SalesOrderLineItemNumber											AS 'SnapshotSalesOrderLineItemNumber',
			s.SalesOrderOrderDate												AS 'SnapshotSalesOrderDate',
			s.SalesOrderOrderLocationID,
			s.SalesOrderCustomerID,
			s.SalesOrderPartDetailID,
			s.SalesOrderWeight													AS 'SnapshotPlannedWeight',
			s.SalesOrderExtendedPrice + COALESCE(s.SalesOrderChargePrice, 0)	AS 'SnapshotPlannedAmount',

			i.InvoiceID															AS 'SnapshotInvoiceID',
			i.InvoiceItemID														AS 'SnapshotInvoiceItemID',
			i.InvoiceRowNum														AS 'SnapshotInvoiceLineItemNumber',
			i.InvoiceDate														AS 'SnapshotActualShipDate',
			i.InvoiceWeight														AS 'SnapshotActualWeight',
			i.InvoiceExtendedPrice + COALESCE(i.InvoiceChargePrice, 0)
			+COALESCE(i.InvoiceTaxCollectable, 0)								AS 'SnapshotActualAmount'

			INTO #NoShip
			FROM Reporting.Snapshot_SalesOrder AS s

				LEFT JOIN Summary.Invoice AS i
					ON i.SalesOrderID = s.SalesOrderID
						AND i.SalesOrderItemID = s.SalesOrderItemID

			WHERE	s.Snapshot_SalesOrderDate = @_CompareDate
				AND (i.InvoiceId IS NULL 
					OR (i.InvoiceDate > @_CompareDate AND i.InvoiceStatus IN (2,4,5)));



		--Other
		IF OBJECT_ID('tempdb..#Other') IS NOT NULL
			DROP TABLE #Other;

		SELECT
			'Other'																AS 'SnapshotCategory',
			s.Snapshot_SalesOrderDate											AS 'SnapshotPlannedShipDate',
			s.SalesOrderID														AS 'SnapshotSalesOrderID',
			s.SalesOrderItemID													AS 'SnapshotSalesOrderItemID',
			s.SalesOrderLineItemNumber											AS 'SnapshotSalesOrderLineItemNumber',
			s.SalesOrderOrderDate												AS 'SnapshotSalesOrderDate',
			s.SalesOrderOrderLocationID,
			s.SalesOrderCustomerID,
			s.SalesOrderPartDetailID,
			s.SalesOrderWeight													AS 'SnapshotPlannedWeight',
			s.SalesOrderExtendedPrice + COALESCE(s.SalesOrderChargePrice, 0)	AS 'SnapshotPlannedAmount',

			i.InvoiceID															AS 'SnapshotInvoiceID',
			i.InvoiceItemID														AS 'SnapshotInvoiceItemID',
			i.InvoiceRowNum														AS 'SnapshotInvoiceLineItemNumber',
			i.InvoiceDate														AS 'SnapshotActualShipDate',
			i.InvoiceWeight														AS 'SnapshotActualWeight',
			i.InvoiceExtendedPrice + COALESCE(i.InvoiceChargePrice, 0)
			+COALESCE(i.InvoiceTaxCollectable, 0)								AS 'SnapshotActualAmount'

			INTO #Other
			FROM Reporting.Snapshot_SalesOrder AS s

				LEFT JOIN #Early AS e
					ON e.SnapshotSalesOrderID = s.SalesOrderID
						AND e.SnapshotSalesOrderItemID = s.SalesOrderItemID

				LEFT JOIN #NoShip AS n
					ON n.SnapshotSalesOrderID = s.SalesOrderID
						AND n.SnapshotSalesOrderItemID = s.SalesOrderItemID

				LEFT JOIN #AsPlanned AS a
					ON a.SnapshotSalesOrderID = s.SalesOrderID
						AND a.SnapshotSalesOrderItemID = s.SalesOrderItemID

				LEFT JOIN Summary.Invoice AS i
					ON i.SalesOrderID = s.SalesOrderID
						AND i.SalesOrderItemID = s.SalesOrderItemID

			WHERE	s.Snapshot_SalesOrderDate = @_CompareDate
				AND e.SnapshotSalesOrderID IS NULL 
				AND n.SnapshotSalesOrderID IS NULL
				AND a.SnapshotSalesOrderID IS NULL;



		--Union All and snapshot data to remove SO's with multiple shipments
		IF OBJECT_ID('tempdb..#Snapshot') IS NOT NULL
			DROP TABLE #Snapshot;

		SELECT * INTO #Snapshot FROM #Early
		UNION ALL
		SELECT * FROM #NoShip
		UNION ALL
		SELECt * FROM #AsPlanned
		UNION ALL
		SELECT * FROM #Other;



		--Find dups and it's total actual info
		--Dups happen because planned SO can have multiple invoices
		IF OBJECT_ID('tempdb..#Dups') IS NOT NULL
			DROP TABLE #Dups;

		SELECT
			s.SnapshotSalesOrderID		AS 'DupSalesOrderID',
			s.SnapshotSalesOrderItemID	AS 'DupSalesOrderItemID',
			SUM(s.SnapshotActualWeight) AS 'TotalActualWeight',
			SUM(s.SnapshotActualAmount) AS 'TotalActualAmount'
			
			INTO #Dups
			FROM #Snapshot AS s
			GROUP BY
				s.SnapshotSalesOrderID,
				s.SnapshotSalesOrderItemID

			HAVING COUNT(*) > 1;


		
		--Get Dups % and rank them by acutal ship date
		IF OBJECT_ID('tempdb..#Percentage') IS NOT NULL
			DROP TABLE #Percentage;

		SELECT
			s.SnapshotSalesOrderID,
			s.SnapshotSalesOrderItemID,
			s.SnapshotInvoiceID,
			s.SnapshotInvoiceItemID,
			s.SnapshotActualShipDate,
			s.SnapshotActualWeight / d.TotalActualWeight	AS 'WeightPercentage',
			s.SnapshotActualAmount / d.TotalActualAmount	AS 'AmountPercentage',
			RANK() OVER(PARTITION BY s.SnapshotSalesOrderItemID ORDER BY s.SnapshotActualShipDate DESC) AS 'Dup'

			INTO #Percentage
			FROM #Snapshot AS s

				JOIN #Dups AS d
					ON s.SnapshotSalesOrderID = d.DupSalesOrderID
						AND s.SnapshotSalesOrderItemID = d.DupSalesOrderItemID;

			

		
		--Remove the older dups
		DELETE #Snapshot 
			FROM #Snapshot AS s
				
				JOIN #Percentage AS p
					ON p.SnapshotInvoiceID = s.SnapshotInvoiceID
						AND p.SnapshotInvoiceItemID = s.SnapshotInvoiceItemID

		WHERE p.Dup <> 1;



		--All Planned Sales Orders
		IF OBJECT_ID('tempdb..#Planned') IS NOT NULL
			DROP TABLE #Planned;

		SELECT
			'Planned'															AS 'SnapshotCategory',
			s.Snapshot_SalesOrderDate											AS 'SnapshotPlannedShipDate',
			s.SalesOrderID														AS 'SnapshotSalesOrderID',
			s.SalesOrderItemID													AS 'SnapshotSalesOrderItemID',
			s.SalesOrderLineItemNumber											AS 'SnapshotSalesOrderLineItemNumber',
			s.SalesOrderOrderDate												AS 'SnapshotSalesOrderDate',
			s.SalesOrderOrderLocationID,
			s.SalesOrderCustomerID,
			s.SalesOrderPartDetailID,
			s.SalesOrderWeight													AS 'SnapshotPlannedWeight',
			s.SalesOrderExtendedPrice + COALESCE(s.SalesOrderChargePrice, 0)	AS 'SnapshotPlannedAmount',

			NULL																AS 'SnapshotInvoiceID',
			NULL																AS 'SnapshotInvoiceItemID',
			NULL																AS 'SnapshotInvoiceLineItemNumber',
			'1900-01-01'														AS 'SnapshotActualShipDate',
			NULL																AS 'SnapshotActualWeight',
			NULL																AS 'SnapshotActualAmount'

			INTO #Planned
			FROM Reporting.Snapshot_SalesOrder AS s
			WHERE s.Snapshot_SalesOrderDate = @_CompareDate;



		--Apply percentages to buckets and planned for dups
		UPDATE s SET
			s.SnapshotPlannedWeight = s.SnapshotPlannedWeight * p.WeightPercentage,
			s.SnapshotPlannedAmount = s.SnapshotPlannedAmount * p.AmountPercentage

			FROM #Snapshot AS s
				
				JOIN #Percentage AS p
					ON p.SnapshotInvoiceID = s.SnapshotInvoiceID
						AND p.SnapshotInvoiceItemID = s.SnapshotInvoiceItemID

			WHERE Dup = 1;



		UPDATE pl SET
			pl.SnapshotPlannedWeight = pl.SnapshotPlannedWeight * p.WeightPercentage,
			pl.SnapshotPlannedAmount = pl.SnapshotPlannedAmount * p.AmountPercentage

			FROM #Planned AS pl
				
				JOIN #Percentage AS p
					ON p.SnapshotSalesOrderID = pl.SnapshotSalesOrderID
						AND p.SnapshotSalesOrderItemID = pl.SnapshotSalesOrderItemID

			WHERE Dup = 1;
		


		--Sales Order Created and Shipped Same Day
		IF OBJECT_ID('tempdb..#SameDay') IS NOT NULL
			DROP TABLE #SameDay;

		SELECT
			'Sales Order Created and Shipped Same Day'							AS 'SnapshotCategory',
			COALESCE(s.SalesOrderLineItemShipmentDate, s.SalesOrderShipmentDate)AS 'SnapshotPlannedShipDate',
			s.SalesOrderID														AS 'SnapshotSalesOrderID',
			s.SalesOrderItemID													AS 'SnapshotSalesOrderItemID',
			s.SalesOrderLineItemNumber											AS 'SnapshotSalesOrderLineItemNumber',
			s.SalesOrderOrderDate												AS 'SnapshotSalesOrderDate',
			s.SalesOrderOrderLocationID,
			s.SalesOrderCustomerID,
			s.SalesOrderPartDetailID,
			s.SalesOrderWeight													AS 'SnapshotPlannedWeight',
			s.SalesOrderExtendedPrice + COALESCE(s.SalesOrderChargePrice, 0)	AS 'SnapshotPlannedAmount',

			i.InvoiceID															AS 'SnapshotInvoiceID',
			i.InvoiceItemID														AS 'SnapshotInvoiceItemID',
			i.InvoiceRowNum														AS 'SnapshotInvoiceLineItemNumber',
			i.InvoiceDate														AS 'SnapshotActualShipDate',
			i.InvoiceWeight														AS 'SnapshotActualWeight',
			i.InvoiceExtendedPrice + COALESCE(i.InvoiceChargePrice, 0)
			+COALESCE(i.InvoiceTaxCollectable, 0)								AS 'SnapshotActualAmount'

			INTO #SameDay
			FROM Summary.SalesOrder AS s
		
				JOIN Summary.Invoice AS i
					ON i.SalesOrderID = s.SalesOrderID
						AND i.SalesOrderItemID = s.SalesOrderItemID

			WHERE CONVERT(DATE, s.SalesOrderCreatedDate) = @_CompareDate
				AND i.InvoiceDate = @_CompareDate
				AND i.InvoiceStatus IN (2,4,5);



		--Sales Order Shipped Earlier than Planned
		IF OBJECT_ID('tempdb..#EarlierThanPlanned') IS NOT NULL
			DROP TABLE #EarlierThanPlanned;

		SELECT
			'Sales Order Shipped Earlier than Planned'								AS 'SnapshotCategory',
			COALESCE(s.SalesOrderLineItemShipmentDate, s.SalesOrderShipmentDate)	AS 'SnapshotPlannedShipDate',
			s.SalesOrderID															AS 'SnapshotSalesOrderID',
			s.SalesOrderItemID														AS 'SnapshotSalesOrderItemID',
			s.SalesOrderLineItemNumber												AS 'SnapshotSalesOrderLineItemNumber',
			s.SalesOrderOrderDate													AS 'SnapshotSalesOrderDate',
			s.SalesOrderOrderLocationID,
			s.SalesOrderCustomerID,
			s.SalesOrderPartDetailID,
			s.SalesOrderWeight														AS 'SnapshotPlannedWeight',
			s.SalesOrderExtendedPrice + COALESCE(s.SalesOrderChargePrice, 0)		AS 'SnapshotPlannedAmount',

			i.InvoiceID																AS 'SnapshotInvoiceID',
			i.InvoiceItemID															AS 'SnapshotInvoiceItemID',
			i.InvoiceRowNum															AS 'SnapshotInvoiceLineItemNumber',
			i.InvoiceDate															AS 'SnapshotActualShipDate',
			i.InvoiceWeight															AS 'SnapshotActualWeight',
			i.InvoiceExtendedPrice + COALESCE(i.InvoiceChargePrice, 0)
			+COALESCE(i.InvoiceTaxCollectable, 0)									AS 'SnapshotActualAmount'

			INTO #EarlierThanPlanned
			FROM Summary.SalesOrder AS s
		
				JOIN Summary.Invoice AS i
					ON i.SalesOrderID = s.SalesOrderID
						AND i.SalesOrderItemID = s.SalesOrderItemID

			WHERE COALESCE(s.SalesOrderLineItemShipmentDate, s.SalesOrderShipmentDate) > @_CompareDate
				AND i.InvoiceDate = @_CompareDate
				AND i.InvoiceStatus IN (2,4,5);



		--Remove records already identified in snapshot
		DELETE #EarlierThanPlanned
			FROM #EarlierThanPlanned AS e
					
				JOIN #Snapshot as s
					ON s.SnapshotSalesOrderID = e.SnapshotSalesOrderID
					AND s.SnapshotSalesOrderItemID = e.SnapshotSalesOrderItemID



		--Sales Order Shipped Later than Planned
		IF OBJECT_ID('tempdb..#LaterThanPlanned') IS NOT NULL
			DROP TABLE #LaterThanPlanned;

		SELECT
			'Sales Order Shipped Later than Planned'								AS 'SnapshotCategory',
			COALESCE(s.SalesOrderLineItemShipmentDate, s.SalesOrderShipmentDate)	AS 'SnapshotPlannedShipDate',
			s.SalesOrderID															AS 'SnapshotSalesOrderID',
			s.SalesOrderItemID														AS 'SnapshotSalesOrderItemID',
			s.SalesOrderLineItemNumber												AS 'SnapshotSalesOrderLineItemNumber',
			s.SalesOrderOrderDate													AS 'SnapshotSalesOrderDate',
			s.SalesOrderOrderLocationID,
			s.SalesOrderCustomerID,
			s.SalesOrderPartDetailID,
			s.SalesOrderWeight														AS 'SnapshotPlannedWeight',
			s.SalesOrderExtendedPrice + COALESCE(s.SalesOrderChargePrice, 0)		AS 'SnapshotPlannedAmount',

			i.InvoiceID																AS 'SnapshotInvoiceID',
			i.InvoiceItemID															AS 'SnapshotInvoiceItemID',
			i.InvoiceRowNum															AS 'SnapshotInvoiceLineItemNumber',
			i.InvoiceDate															AS 'SnapshotActualShipDate',
			i.InvoiceWeight															AS 'SnapshotActualWeight',
			i.InvoiceExtendedPrice + COALESCE(i.InvoiceChargePrice, 0)
			+COALESCE(i.InvoiceTaxCollectable, 0)									AS 'SnapshotActualAmount'

			INTO #LaterThanPlanned
			FROM Summary.SalesOrder AS s
		
				JOIN Summary.Invoice AS i
					ON i.SalesOrderID = s.SalesOrderID
						AND i.SalesOrderItemID = s.SalesOrderItemID

			WHERE COALESCE(s.SalesOrderLineItemShipmentDate, s.SalesOrderShipmentDate) < @_CompareDate
				AND i.InvoiceDate = @_CompareDate
				AND i.InvoiceStatus IN (2,4,5);



		--Remove records already identified in snapshot
		DELETE #LaterThanPlanned
			FROM #LaterThanPlanned AS l
					
				JOIN #Snapshot as s
					ON s.SnapshotSalesOrderID = l.SnapshotSalesOrderID
					AND s.SnapshotSalesOrderItemID = l.SnapshotSalesOrderItemID;



		--Sales Order Modified and Shipped Same Day
		IF OBJECT_ID('tempdb..#ModOther') IS NOT NULL
			DROP TABLE #ModOther;

		SELECT
			'Other'																AS 'SnapshotCategory',
			COALESCE(s.SalesOrderLineItemShipmentDate, s.SalesOrderShipmentDate)AS 'SnapshotPlannedShipDate',
			s.SalesOrderID														AS 'SnapshotSalesOrderID',
			s.SalesOrderItemID													AS 'SnapshotSalesOrderItemID',
			s.SalesOrderLineItemNumber											AS 'SnapshotSalesOrderLineItemNumber',
			s.SalesOrderOrderDate												AS 'SnapshotSalesOrderDate',
			s.SalesOrderOrderLocationID,
			s.SalesOrderCustomerID,
			s.SalesOrderPartDetailID,
			s.SalesOrderWeight													AS 'SnapshotPlannedWeight',
			s.SalesOrderExtendedPrice + COALESCE(s.SalesOrderChargePrice, 0)	AS 'SnapshotPlannedAmount',

			i.InvoiceID															AS 'SnapshotInvoiceID',
			i.InvoiceItemID														AS 'SnapshotInvoiceItemID',
			i.InvoiceRowNum														AS 'SnapshotInvoiceLineItemNumber',
			i.InvoiceDate														AS 'SnapshotActualShipDate',
			i.InvoiceWeight														AS 'SnapshotActualWeight',
			i.InvoiceExtendedPrice + COALESCE(i.InvoiceChargePrice, 0)
			+COALESCE(i.InvoiceTaxCollectable, 0)								AS 'SnapshotActualAmount'

			INTO #ModOther
			FROM Summary.SalesOrder AS s
		
				JOIN Summary.Invoice AS i
					ON i.SalesOrderID = s.SalesOrderID
						AND i.SalesOrderItemID = s.SalesOrderItemID

			WHERE CONVERT(DATE, s.SalesOrderModifiedDate) = @_CompareDate
				AND i.InvoiceDate = @_CompareDate
				AND i.InvoiceStatus IN (2,4,5);



		--Remove records already identified
		DELETE #ModOther
			FROM #ModOther AS m
					
				JOIN #Snapshot as s
					ON s.SnapshotSalesOrderID = m.SnapshotSalesOrderID
					AND s.SnapshotSalesOrderItemID = m.SnapshotSalesOrderItemID;

		DELETE #ModOther
			FROM #ModOther AS m
					
				JOIN #SameDay as s
					ON s.SnapshotSalesOrderID = m.SnapshotSalesOrderID
					AND s.SnapshotSalesOrderItemID = m.SnapshotSalesOrderItemID;

		DELETE #ModOther
			FROM #ModOther AS m
					
				JOIN #EarlierThanPlanned as e
					ON e.SnapshotSalesOrderID = m.SnapshotSalesOrderID
					AND e.SnapshotSalesOrderItemID = m.SnapshotSalesOrderItemID;

		DELETE #ModOther
			FROM #ModOther AS m
					
				JOIN #LaterThanPlanned as s
					ON s.SnapshotSalesOrderID = m.SnapshotSalesOrderID
					AND s.SnapshotSalesOrderItemID = m.SnapshotSalesOrderItemID;



		--Union All and return dataset with dimensions
		IF OBJECT_ID('tempdb..#ResultSet') IS NOT NULL
			DROP TABLE #ResultSet;

		SELECT * INTO #ResultSet FROM #Snapshot
		UNION ALL
		SELECT * FROM #Planned
		UNION ALL
		SELECT * FROM #SameDay
		UNION ALL
		SELECT * FROM #EarlierThanPlanned
		UNION ALL
		SELECT * FROM #LaterThanPlanned
		UNION ALL
		SELECT * FROM #ModOther;



		--Return Dataset
		SELECT
			rs.SnapshotCategory														AS 'SnapshotCategory',
			rs.SnapshotPlannedShipDate												AS 'SnapshotPlannedShipDate',
			rs.SnapshotSalesOrderID													AS 'SnapshotSalesOrderID',
			rs.SnapshotSalesOrderItemID												AS 'SnapshotSalesOrderItemID',
			rs.SnapshotSalesOrderLineItemNumber										AS 'SnapshotSalesOrderLineItemNumber',
			rs.SnapshotSalesOrderDate												AS 'SnapshotSalesOrderDate',
			l.LocationCode															AS 'SnapshotLocation',
			ccom.CompanyCode														AS 'SnapshotCustomerCode',		
			REPLACE(COALESCE(csn.CustomerShortName,ccom.CompanyName), ',', ';')		AS 'SnapshotCustomerName',
			pcom.CompanyCode														AS 'SnapshotMasterCustomerCode',	
			REPLACE(COALESCE(psn.CustomerShortName, pcom.CompanyName), ',', ';')	AS 'SnapshotMasterCustomerName',
			COALESCE(srm.SalesRepName, t.TerritoryName)								AS 'SnapshotSalesRep',			
			r.RegionName				 											AS 'SnapshotRegion',		
			p.PartNo																AS 'SnapshotPartNumber',			
			REPLACE(REPLACE(p.Description, ',', ';'), '"', '')						AS 'SnapshotPartDescription',	
			REPLACE(pt.PartTypeCode, ',', ';')										AS 'SnapshotPartType',	
			REPLACE(REPLACE(pcat.PartCategoryCode, ',', ';'), '"', '')				AS 'SnapshotPartCategory',	
			REPLACE(pclass.PartClassCode, ',', ';')									AS 'SnapshotPartClass',
			rs.SnapshotPlannedWeight												AS 'SnapshotPlannedWeight',
			rs.SnapshotPlannedAmount												AS 'SnapshotPlannedAmount',
																			
			rs.SnapshotInvoiceID													AS 'SnapshotInvoiceID',
			rs.SnapshotInvoiceItemID												AS 'SnapshotInvoiceItemID',
			rs.SnapshotInvoiceLineItemNumber										AS 'SnapshotInvoiceLineItemNumber',
			rs.SnapshotActualShipDate												AS 'SnapshotActualShipDate',
			rs.SnapshotActualWeight													AS 'SnapshotActualWeight',
			rs.SnapshotActualAmount													AS 'SnapshotActualAmount'
	
			FROM #ResultSet AS rs
		
				--Location
				JOIN dbo.Location AS l
					ON l.LocationId = rs.SalesOrderOrderLocationID

				--Child Customer
				JOIN dbo.Customer AS ccus
					ON ccus.CustomerId = rs.SalesOrderCustomerID

				JOIN dbo.Company AS ccom
					ON ccom.CompanyID = ccus.CompanyId

				LEFT JOIN Reporting.CustomerShortNames AS csn
					ON csn.CustomerCode = ccom.CompanyCode

				--Master Customer
				LEFT JOIN dbo.customer AS pcus
					ON pcus.CustomerId = ccus.ParentCustomerId
		
				LEFT JOIN dbo.Company AS pcom
					ON pcom.CompanyID = pcus.CompanyId

				LEFT JOIN Reporting.CustomerShortNames AS psn
					ON psn.CustomerCode = pcom.CompanyCode

				--Sales Rep
				LEFT JOIN dbo.CustomerSalesRep AS sr
					ON sr.CustomerId = ccus.CustomerId
				
				LEFT JOIN dbo.Employee AS e
					ON e.EmployeeId = sr.EmployeeId

				LEFT JOIN dbo.Territory AS t
					ON t.TerritoryName = e.FirstName + ' ' + e.LastName

				--Region
				LEFT JOIN dbo.Region AS r
					ON r.RegionId = t.RegionId

				LEFT JOIN Reporting.SalesRepMapping AS srm
					ON srm.TerritoryId = t.TerritoryId

				--Part
				LEFT JOIN dbo.PartDetail AS pd	
					ON pd.PartDetailId = rs.SalesOrderPartDetailID

				LEFT JOIN dbo.Part AS p
					ON p.PartId = pd.PartId

				LEFT JOIN dbo.PartType AS pt
					ON pt.PartTypeId = p.ProductLineId

				LEFT JOIN dbo.PartCategory AS pcat
					ON pcat.PartCategoryId = p.ProductCategoryId

				LEFT JOIN dbo.PartClass AS pclass
					ON pclass.PartClassId = p.ProductClassId;
		
	COMMIT

END
