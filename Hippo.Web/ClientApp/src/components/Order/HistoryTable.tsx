import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { HistoryModel } from "../../types";
import { ReactTable } from "../../Shared/ReactTable";
import { convertToPacificTime } from "../../util/DateHelper";
import { createColumnHelper } from "@tanstack/react-table";
import { authenticatedFetch } from "../../util/api";
import { useParams } from "react-router-dom";

interface HistoryTableProps {
  numberOfRows: number;
  showLinkToAll: boolean;
}

export const HistoryTable: React.FC<HistoryTableProps> = ({
  numberOfRows,
  showLinkToAll,
}) => {
  const [histories, setHistories] = useState<HistoryModel[]>([]);
  const { cluster, orderId } = useParams();
  const navigate = useNavigate();

  useEffect(() => {
    const fetchHistories = async () => {
      console.log(
        `/api/${cluster}/order/GetHistories/${orderId}?max=${numberOfRows}`,
      );
      const response = await authenticatedFetch(
        `/api/${cluster}/order/GetHistories/${orderId}?max=${numberOfRows}`,
      );

      if (response.ok) {
        const data = await response.json();

        setHistories(data);
      } else {
        alert("Error fetching histories");
      }
    };

    fetchHistories();
  }, [cluster, numberOfRows, orderId]);

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
      <small>Last {numberOfRows} or fewer Actions</small>{" "}
      {showLinkToAll ? (
        <button
          onClick={() => navigate(`/${cluster}/order/history/${orderId}`)}
          className="float-right"
        >
          View All
        </button>
      ) : (
        <button
          onClick={() => navigate(`/${cluster}/order/details/${orderId}`)}
          className="float-right"
        >
          Back to Order Details
        </button>
      )}
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
