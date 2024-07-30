import { useEffect, useState, useMemo, useCallback } from "react";
import { RequestModel } from "../../types";
import { RejectRequest } from "../../Shared/RejectRequest";
import { authenticatedFetch } from "../../util/api";
import { usePromiseNotification } from "../../util/Notifications";
import { useParams } from "react-router-dom";
import { ReactTable } from "../../Shared/ReactTable";
import { SplitCamelCase, getGroupModelString } from "../../util/StringHelpers";
import { GroupNameWithTooltip } from "../Group/GroupNameWithTooltip";
import { isAccountRequest } from "../../util/TypeChecks";
import { createColumnHelper } from "@tanstack/react-table";

export const Requests = () => {
  // get all accounts that need approval and list them
  // allow user to approve or reject each request

  const [requests, setRequests] = useState<RequestModel[]>();
  const [requestApproving, setRequestApproving] = useState<number>();
  const [notification, setNotification] = usePromiseNotification();

  const { cluster } = useParams();

  useEffect(() => {
    const fetchRequests = async () => {
      const response = await authenticatedFetch(
        `/api/${cluster}/request/pending`,
      );

      if (response.ok) {
        setRequests((await response.json()) as RequestModel[]);
      }
    };

    fetchRequests();
  }, [cluster]);

  const handleApprove = useCallback(
    async (request: RequestModel) => {
      setRequestApproving(request.id);

      const req = authenticatedFetch(
        `/api/${cluster}/request/approve/${request.id}`,
        {
          method: "POST",
        },
      );

      setNotification(
        req,
        "Approving",
        "Request Approved. Please allow 2 to 3 hours for changes to take place.",
      );

      const response = await req;
      if (response.ok) {
        setRequestApproving(undefined);

        // remove the request from the list
        setRequests(requests?.filter((a) => a.id !== request.id));
      }
    },
    [requests, cluster, setNotification],
  );

  const handleReject = useCallback(
    async (request: RequestModel) => {
      // remove the request from the list
      setRequests(requests?.filter((a) => a.id !== request.id));
    },
    [requests],
  );

  const columnHelper = createColumnHelper<RequestModel>();

  const columns = [
    columnHelper.accessor((request) => SplitCamelCase(request.action), {
      header: "Request",
      meta: {
        filterVariant: "select",
      },
    }),
    columnHelper.accessor("requesterName", {
      header: "Name",
      id: "Name", // id required for only this column for some reason
    }),
    columnHelper.accessor("requesterEmail", {
      header: "Email",
    }),
    columnHelper.accessor((row) => getGroupModelString(row.groupModel), {
      header: "Group",
      cell: (props) => (
        <GroupNameWithTooltip
          group={props.row.original.groupModel}
          showDisplayName={false}
        />
      ),
      meta: {
        exportFn: (request) => request.groupModel.displayName,
      },
    }),
    columnHelper.accessor(
      (request) => isAccountRequest(request) && request.data.supervisingPI,
      {
        header: "Supervising PI",
      },
    ),
    columnHelper.accessor(
      (request) =>
        isAccountRequest(request) && request.data.accessTypes.join(", "),
      {
        header: "Access Types",
        meta: {
          filterVariant: "select",
        },
      },
    ),
    columnHelper.display({
      id: "actions",
      header: "Action",
      cell: (props) => (
        <>
          <button
            id="approveButton"
            disabled={notification.pending}
            onClick={() => handleApprove(props.row.original)}
            className="btn btn-primary"
          >
            {requestApproving === props.row.original.id
              ? "Approving..."
              : "Approve"}
          </button>
          {requestApproving !== props.row.original.id && (
            <RejectRequest
              request={props.row.original}
              removeAccount={() => handleReject(props.row.original)}
              updateUrl={`/api/${cluster}/request/reject/`}
              disabled={notification.pending}
            ></RejectRequest>
          )}
        </>
      ),
    }),
  ];

  const accountsData = useMemo(() => requests ?? [], [requests]);

  if (requests === undefined) {
    return (
      <div className="row justify-content-center">
        <div className="col-md-12">Loading...</div>
      </div>
    );
  } else {
    return (
      <div className="row justify-content-center">
        <div className="col-md-12">
          <p>There are {requests.length} request(s) awaiting approval</p>
          <ReactTable
            columns={columns}
            data={accountsData}
            initialState={{
              sorting: [
                { id: "Request", desc: false },
                { id: "Name", desc: false },
              ],
            }}
          />
        </div>
      </div>
    );
  }
};
