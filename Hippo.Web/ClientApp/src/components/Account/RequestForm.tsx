import { useContext, useEffect, useState } from "react";
import "react-bootstrap-typeahead/css/Typeahead.css";
import { useHistory } from "react-router-dom";
import AppContext from "../../Shared/AppContext";
import { Account, RequestPostModel } from "../../types";
import { authenticatedFetch } from "../../util/api";
import { Typeahead } from "react-bootstrap-typeahead";
import { usePromiseNotification } from "../../util/Notifications";

export const RequestForm = () => {
  const [context, setContext] = useContext(AppContext);
  const [notification, setNotification] = usePromiseNotification();

  const [sponsors, setSponsors] = useState<Account[]>([]);
  const [request, setRequest] = useState<RequestPostModel>({
    sponsorId: 0,
    sshKey: "",
  });

  const history = useHistory();

  // load up possible sponsors
  useEffect(() => {
    const fetchSponsors = async () => {
      const response = await authenticatedFetch("/api/account/sponsors");

      const sponsorResult = await response.json();

      if (response.ok) {
        setSponsors(sponsorResult);
      }
    };

    fetchSponsors();
  }, []);

  const handleSubmit = async () => {
    const req = authenticatedFetch("/api/account/create", {
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
      const newAccount = await response.json();
      setContext((ctx) => ({ ...ctx, account: newAccount }));
      history.replace("/"); // could also push straight to pending, but home will redirect there immediately anyway
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
          You don't seem to have an account on Farm yet. If you'd like access,
          please answer the&nbsp;questions&nbsp;below
        </p>
        <hr />
        <div className="form-group">
          <label>Who is sponsoring your account?</label>
          <Typeahead
            id="sponsorLookup"
            labelKey="name"
            placeholder="Select a sponsor"
            onChange={(selected) => {
              if (selected.length > 0) {
                setRequest((r) => ({
                  ...r,
                  sponsorId: Object.values(selected[0])[0],
                }));
              } else {
                setRequest((r) => ({ ...r, sponsorId: 0 }));
              }
            }}
            options={sponsors.map(({ id, name }) => ({ id, name }))}
          />
          <p className="form-helper">
            Your sponsor is probably your PI or your Department. You can filter
            this list by typing in it.
          </p>
          <p className="form-helper">
            If you don't see your sponsor, you may contact IT help to request
            they be added.{" "}
            <a href="mailto: ithelp@ucdavis.edu?subject=Please add my sponsor to the Farm Cluster&body=Sponsor Name:  %0D%0ASponsor Email: ">
              Click here to contact IET Help
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
              -----BEGIN RSA PRIVATE KEY-----ABC123-----END RSA PRIVATE KEY-----
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
