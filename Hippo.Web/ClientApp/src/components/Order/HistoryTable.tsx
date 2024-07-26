import React, { useEffect, useState } from "react";
import { HistoryModel } from "../../types";
import { ReactTable } from "../../Shared/ReactTable";
import { convertToPacificTime } from "../../util/DateHelper";
import { createColumnHelper } from "@tanstack/react-table";
import { authenticatedFetch } from "../../util/api";
import { useParams } from "react-router-dom";

export const HistoryTable: React.FC = () => {
  const [histories, setHistories] = useState<HistoryModel[]>([]);
  const { cluster, orderId } = useParams();

  useEffect(() => {
    const fetchHistories = async () => {
      const response = await authenticatedFetch(
        `/api/${cluster}/order/GetHistories/${orderId}`,
      );

      if (response.ok) {
        const data = await response.json();

        setHistories(data);
      } else {
        alert("Error fetching histories");
      }
    };

    fetchHistories();
  }, [cluster, orderId]);

  const historyColumnHelper = createColumnHelper<HistoryModel>();

  const historyColumns = [
    historyColumnHelper.accessor("actedDate", {
      header: "Date",
      id: "actedDate",
      cell: (value) => (
        <span>{convertToPacificTime(value.row.original.actedDate)}</span>
      ),
    }),
    historyColumnHelper.accessor("actedBy", {
      header: "Actor",
      id: "actedBy",
      cell: (value) => (
        <>
          {value.row.original.actedBy ? (
            <>
              {value.row.original.actedBy.name} (
              {value.row.original.actedBy.email})
            </>
          ) : (
            <>System</>
          )}
        </>
      ),
    }),
    historyColumnHelper.accessor("status", { header: "Status", id: "status" }),
    historyColumnHelper.accessor("details", {
      header: "Details",
      id: "details",
    }),
  ];

  if (!histories) {
    return <div>Loading Histories...</div>;
  }

  return (
    <>
      <h2>History</h2>
      <small>Last 5 Actions</small>
      <ReactTable
        columns={historyColumns}
        data={histories}
        initialState={{
          sorting: [
            {
              id: "actedDate",
              desc: true,
            },
          ],
        }}
      />
    </>
  );
};
