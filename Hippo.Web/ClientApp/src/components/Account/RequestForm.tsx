import { useContext, useEffect, useState } from "react";
import "react-bootstrap-typeahead/css/Typeahead.css";
import { useHistory, useParams } from "react-router-dom";
import AppContext from "../../Shared/AppContext";
import {
  GroupModel,
  AccountModel,
  IRouteParams,
  RequestPostModel,
} from "../../types";
import { authenticatedFetch } from "../../util/api";
import { Typeahead } from "react-bootstrap-typeahead";
import { usePromiseNotification } from "../../util/Notifications";

export const RequestForm = () => {
  const [context, setContext] = useContext(AppContext);
  const [notification, setNotification] = usePromiseNotification();

  const [groups, setGroups] = useState<GroupModel[]>([]);
  const [request, setRequest] = useState<RequestPostModel>({
    groupId: 0,
    sshKey: "",
  });

  const history = useHistory();
  const { cluster } = useParams<IRouteParams>();

  // load up possible groups
  useEffect(() => {
    const fetchGroups = async () => {
      const response = await authenticatedFetch(
        `/api/${cluster}/account/groups`
      );

      const groupsResult = await response.json();

      if (response.ok) {
        setGroups(groupsResult);
      }
    };

    fetchGroups();
  }, [cluster]);

  const handleSubmit = async () => {
    const req = authenticatedFetch(`/api/${cluster}/account/create`, {
      method: "POST",
      body: JSON.stringify(request),
    });

    setNotification(req, "Saving", "Request Created", async (r) => {
      if (r.status === 400) {
        const errorText = await response.text(); //Bad Request Text
        return errorText;
      } else {
        return "An error happened, please try again.";
      }
    });

    const response = await req;

    if (response.ok) {
      const newAccount = (await response.json()) as AccountModel;

      setContext((ctx) => ({
        ...ctx,
        accounts: [...ctx.accounts, { ...newAccount }],
      }));
      history.replace(`/${cluster}/pendingapproval`);
    }
  };

  return (
    <div className="row justify-content-center">
      <div className="col-md-8">
        <h3>
          Welcome,{" "}
          <span className="status-color">{context.user.detail.firstName}</span>
        </h3>
        <p>
          You don't seem to have an account on this cluster yet. If you'd like
          access, please answer the&nbsp;questions&nbsp;below
        </p>
        <hr />
        <div className="form-group">
          <label>Who is sponsoring your account?</label>
          <Typeahead
            id="groupLookup"
            labelKey="name"
            placeholder="Select a group"
            onChange={(selected) => {
              if (selected.length > 0) {
                setRequest((r) => ({
                  ...r,
                  groupId: Object.values(selected[0])[0],
                }));
              } else {
                setRequest((r) => ({ ...r, groupId: 0 }));
              }
            }}
            options={groups.map(({ id, displayName }) => ({
              id,
              name: displayName,
            }))}
          />
          <p className="form-helper">
            Your group is probably named after your PI or your Department. You
            can filter this list by typing in it.
            <br />
            If you don't see your group, you may contact HPC help to request it
            be added.{" "}
            <a
              href={`mailto: hpc-help@ucdavis.edu?subject=Please add a group to the ${cluster} cluster&body=Group Name:  %0D%0API or Dept Email: `}
            >
              Click here to contact HPC Help
            </a>
          </p>
        </div>
        <div className="form-group">
          <label className="form-label">What is your Public SSH key</label>
          <textarea
            className="form-control"
            id="sharedKey"
            required
            onChange={(e) =>
              setRequest((r) => ({ ...r, sshKey: e.target.value }))
            }
          ></textarea>
          <p className="form-helper">
            Paste all of the text from your public SSH file here. Example:
            <br></br>
            <code>
              ssh-rsa AAAAB3NzaC1yc....NrRFi9wrf+M7Q== fake@addr.local
            </code>
          </p>
        </div>
        <button
          disabled={notification.pending}
          onClick={handleSubmit}
          className="btn btn-primary"
        >
          Submit
        </button>
        <div>
          <br />
          <br />
          <p className="form-helper">
            To generate a ssh key pair please see this link:{" "}
            <a href="https://hpc.ucdavis.edu/faq#ssh-key" target={"blank"}>
              https://hpc.ucdavis.edu/faq#ssh-key
            </a>
          </p>
          <p className="form-helper">
            You will find instructions here for Windows, OS X, and Linux
            environments. Windows users: an openssh or SSH2 formatted ssh public
            key is required. See screenshots.
          </p>
          <p className="form-helper">
            (If your public key resides in the default location for OS X and
            Linux, ~/.ssh/, right click in the Name column of the window that
            will open when you click "Choose File" above. Select "Show Hidden
            Files".)
          </p>
        </div>
      </div>
    </div>
  );
};
