import { useEffect, useState } from 'react';

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL || '').replace(/\/$/, '');
const TOKEN_KEY = 'podcastPlatformToken';
const USER_KEY = 'podcastPlatformUser';

function loadJson(key) {
  try {
    const value = localStorage.getItem(key);
    return value ? JSON.parse(value) : null;
  } catch {
    return null;
  }
}

function getErrorMessage(error) {
  if (typeof error === 'string') return error;
  if (error?.message) return error.message;
  return 'Something went wrong';
}

async function parseResponse(response) {
  const text = await response.text();
  let data = null;

  if (text) {
    try {
      data = JSON.parse(text);
    } catch {
      data = text;
    }
  }

  if (!response.ok) {
    const message = data?.message || data?.title || text || `Request failed (${response.status})`;
    throw new Error(message);
  }

  return data;
}

function createApi(token) {
  async function request(path, options = {}) {
    const headers = new Headers(options.headers || {});

    if (token) headers.set('Authorization', `Bearer ${token}`);
    if (options.body && !(options.body instanceof FormData)) {
      headers.set('Content-Type', 'application/json');
    }

    const response = await fetch(`${API_BASE_URL}${path}`, {
      ...options,
      headers,
    });

    return parseResponse(response);
  }

  return {
    get: (path) => request(path),
    post: (path, body) => request(path, { method: 'POST', body: body instanceof FormData ? body : JSON.stringify(body) }),
    patch: (path, body) => request(path, { method: 'PATCH', body: JSON.stringify(body) }),
    put: (path, body) => request(path, { method: 'PUT', body: JSON.stringify(body) }),
    delete: (path) => request(path, { method: 'DELETE' }),
  };
}

function formatDate(value) {
  if (!value) return '';
  return new Intl.DateTimeFormat(undefined, { month: 'short', day: 'numeric', year: 'numeric' }).format(new Date(value));
}

function App() {
  const [token, setToken] = useState(() => localStorage.getItem(TOKEN_KEY));
  const [user, setUser] = useState(() => loadJson(USER_KEY));
  const [podcasts, setPodcasts] = useState([]);
  const [episodes, setEpisodes] = useState([]);
  const [playlists, setPlaylists] = useState([]);
  const [selectedPodcast, setSelectedPodcast] = useState(null);
  const [selectedEpisodes, setSelectedEpisodes] = useState([]);
  const [ratings, setRatings] = useState([]);
  const [ratingStats, setRatingStats] = useState(null);
  const [mySubscriptions, setMySubscriptions] = useState([]);
  const [activeTab, setActiveTab] = useState('discover');
  const [loading, setLoading] = useState(true);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');

  const api = createApi(token);

  async function loadPublicData() {
    setLoading(true);
    setError('');

    try {
      const [podcastData, episodeData, playlistData] = await Promise.all([
        api.get('/api/podcasts'),
        api.get('/api/episodes'),
        api.get('/api/playlists'),
      ]);
      setPodcasts(podcastData || []);
      setEpisodes(episodeData || []);
      setPlaylists(playlistData || []);
      if (!selectedPodcast && podcastData?.length) {
        await selectPodcast(podcastData[0]);
      }
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  }

  async function loadPrivateData() {
    if (!token) return;

    try {
      const [profile, subscriptions] = await Promise.all([
        api.get('/api/auth/profile'),
        api.get('/api/subscriptions/my-subscriptions'),
      ]);
      setUser(profile);
      setMySubscriptions(subscriptions || []);
      localStorage.setItem(USER_KEY, JSON.stringify(profile));
    } catch (err) {
      setError(getErrorMessage(err));
    }
  }

  async function selectPodcast(podcast) {
    setSelectedPodcast(podcast);
    setError('');

    try {
      const [episodeData, ratingData, statsData] = await Promise.all([
        api.get(`/api/episodes/podcast/${podcast.id}`),
        api.get(`/api/ratings/podcast/${podcast.id}`),
        api.get(`/api/ratings/podcast/${podcast.id}/stats`),
      ]);
      setSelectedEpisodes(episodeData || []);
      setRatings(ratingData || []);
      setRatingStats(statsData || null);
    } catch (err) {
      setError(getErrorMessage(err));
    }
  }

  useEffect(() => {
    loadPublicData();
  }, []);

  useEffect(() => {
    loadPrivateData();
  }, [token]);

  function saveSession(authResponse) {
    if (!authResponse?.token || !authResponse?.user) {
      throw new Error(authResponse?.message || 'Authentication failed');
    }

    localStorage.setItem(TOKEN_KEY, authResponse.token);
    localStorage.setItem(USER_KEY, JSON.stringify(authResponse.user));
    setToken(authResponse.token);
    setUser(authResponse.user);
    setMessage(authResponse.message || 'Signed in');
    setActiveTab('dashboard');
  }

  async function handleLogout() {
    try {
      if (token) await api.post('/api/auth/logout', {});
    } catch {
      // Logout is still valid locally if the API call fails or the token expired.
    }

    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    setToken(null);
    setUser(null);
    setMySubscriptions([]);
    setActiveTab('discover');
    setMessage('Signed out');
  }

  async function handleSubmit(action, successMessage) {
    setError('');
    setMessage('');

    try {
      await action();
      setMessage(successMessage);
      await loadPublicData();
      await loadPrivateData();
      if (selectedPodcast) await selectPodcast(selectedPodcast);
    } catch (err) {
      setError(getErrorMessage(err));
    }
  }

  return (
    <div className="app-shell">
      <header className="hero">
        <nav className="topbar">
          <div className="brand">
            <span className="brand-mark">PP</span>
            <span>Podcast Platform</span>
          </div>
          <div className="nav-actions">
            {['discover', 'dashboard', 'create'].map((tab) => (
              <button
                className={activeTab === tab ? 'nav-link active' : 'nav-link'}
                key={tab}
                onClick={() => setActiveTab(tab)}
                type="button"
              >
                {tab}
              </button>
            ))}
            {user ? (
              <button className="button ghost" onClick={handleLogout} type="button">Logout</button>
            ) : (
              <button className="button ghost" onClick={() => setActiveTab('account')} type="button">Login</button>
            )}
          </div>
        </nav>

        <section className="hero-grid">
          <div>
            <p className="eyebrow">Podcast workspace</p>
            <h1>Browse, publish, and manage podcasts.</h1>
            <p className="hero-copy">
              {user ? `Signed in as ${user.userName}.` : 'Sign in to create podcasts, episodes, playlists, ratings, and subscriptions.'}
            </p>
            <div className="hero-actions">
              <button className="button primary" onClick={() => setActiveTab('discover')} type="button">Browse podcasts</button>
              <button className="button" onClick={() => setActiveTab(user ? 'create' : 'account')} type="button">
                {user ? 'Create content' : 'Sign in to create'}
              </button>
            </div>
          </div>
          <div className="stat-card">
            <span>{podcasts.length}</span>
            <p>podcasts available</p>
            <span>{episodes.length}</span>
            <p>episodes ready to play</p>
          </div>
        </section>
      </header>

      <main>
        {(message || error) && (
          <div className={error ? 'notice error' : 'notice'}>{error || message}</div>
        )}

        {activeTab === 'discover' && (
          <DiscoverView
            episodes={episodes}
            loading={loading}
            onSelectPodcast={selectPodcast}
            podcasts={podcasts}
            ratingStats={ratingStats}
            ratings={ratings}
            selectedEpisodes={selectedEpisodes}
            selectedPodcast={selectedPodcast}
            user={user}
            onRate={(payload) => handleSubmit(() => api.post('/api/ratings', payload), 'Rating saved')}
            onSubscribe={(id) => handleSubmit(() => api.post(`/api/subscriptions/subscribe/${id}`, {}), 'Subscribed')}
            onUnsubscribe={(id) => handleSubmit(() => api.delete(`/api/subscriptions/unsubscribe/${id}`), 'Unsubscribed')}
          />
        )}

        {activeTab === 'account' && (
          <AccountView onLogin={(body) => api.post('/api/auth/login', body).then(saveSession)} onRegister={(body) => api.post('/api/auth/register', body).then(saveSession)} />
        )}

        {activeTab === 'dashboard' && (
          <DashboardView episodes={episodes} playlists={playlists} subscriptions={mySubscriptions} user={user} />
        )}

        {activeTab === 'create' && (
          <CreateView
            isSignedIn={Boolean(user)}
            onCreateEpisode={(formData) => handleSubmit(() => api.post('/api/episodes', formData), 'Episode created')}
            onCreatePlaylist={(body) => handleSubmit(() => api.post('/api/playlists', body), 'Playlist created')}
            onCreatePodcast={(formData) => handleSubmit(() => api.post('/api/podcasts', formData), 'Podcast created')}
            podcasts={podcasts}
          />
        )}
      </main>
    </div>
  );
}

function DiscoverView({ episodes, loading, onRate, onSelectPodcast, onSubscribe, onUnsubscribe, podcasts, ratingStats, ratings, selectedEpisodes, selectedPodcast, user }) {
  if (loading) return <section className="panel"><p>Loading podcasts from the backend...</p></section>;

  return (
    <section className="content-grid">
      <div className="panel podcast-list">
        <div className="section-heading">
          <p className="eyebrow">Catalog</p>
          <h2>Podcasts</h2>
        </div>
        {podcasts.length === 0 ? <EmptyState text="No podcasts have been created yet." /> : podcasts.map((podcast) => (
          <button className={selectedPodcast?.id === podcast.id ? 'podcast-row active' : 'podcast-row'} key={podcast.id} onClick={() => onSelectPodcast(podcast)} type="button">
            <img alt="" src={podcast.imageUrl || fallbackImage(podcast.title)} />
            <span>
              <strong>{podcast.title}</strong>
                <small>{podcast.category || 'Uncategorized'} | {podcast.episodeCount || 0} episodes</small>
            </span>
          </button>
        ))}
      </div>

      <div className="panel detail-panel">
        {selectedPodcast ? (
          <>
            <div className="podcast-hero">
              <img alt="" src={selectedPodcast.imageUrl || fallbackImage(selectedPodcast.title)} />
              <div>
                <p className="eyebrow">{selectedPodcast.category || 'Podcast'}</p>
                <h2>{selectedPodcast.title}</h2>
                <p>{selectedPodcast.description || 'No description yet.'}</p>
                <div className="pill-row">
                  <span>{selectedPodcast.subscriberCount || 0} subscribers</span>
                <span>{ratingStats?.averageRating ? ratingStats.averageRating.toFixed(1) : 'No'} rating</span>
                  <span>{formatDate(selectedPodcast.createdAt)}</span>
                </div>
                {user && (
                  <div className="inline-actions">
                    <button className="button primary" onClick={() => onSubscribe(selectedPodcast.id)} type="button">Subscribe</button>
                    <button className="button" onClick={() => onUnsubscribe(selectedPodcast.id)} type="button">Unsubscribe</button>
                  </div>
                )}
              </div>
            </div>

            <RatingForm disabled={!user} onSubmit={(payload) => onRate({ ...payload, podcastId: selectedPodcast.id })} />

            <div className="section-heading compact">
              <p className="eyebrow">Episodes</p>
              <h3>{selectedEpisodes.length} episode{selectedEpisodes.length === 1 ? '' : 's'}</h3>
            </div>
            <EpisodeList episodes={selectedEpisodes} />

            <div className="section-heading compact">
              <p className="eyebrow">Reviews</p>
              <h3>Listener ratings</h3>
            </div>
            {ratings.length === 0 ? <EmptyState text="No ratings yet." /> : ratings.map((rating) => (
              <article className="review" key={rating.id}>
                <strong>{rating.userName}</strong>
                <span>{rating.rating}/5 stars</span>
                <p>{rating.review || 'No written review.'}</p>
              </article>
            ))}
          </>
        ) : (
          <EmptyState text="Select a podcast to see episodes, ratings, and subscription actions." />
        )}
      </div>

      <div className="panel now-playing">
        <div className="section-heading">
          <p className="eyebrow">Latest</p>
          <h2>All episodes</h2>
        </div>
        <EpisodeList episodes={episodes.slice(0, 8)} />
      </div>
    </section>
  );
}

function AccountView({ onLogin, onRegister }) {
  const [mode, setMode] = useState('login');
  const [pending, setPending] = useState(false);
  const [form, setForm] = useState({ fullName: '', userName: '', email: '', password: '', bio: '' });
  const [error, setError] = useState('');

  async function submit(event) {
    event.preventDefault();
    setPending(true);
    setError('');

    try {
      if (mode === 'login') {
        await onLogin({ userName: form.userName, password: form.password });
      } else {
        await onRegister(form);
      }
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setPending(false);
    }
  }

  return (
    <section className="auth-layout">
      <div className="panel auth-copy">
        <p className="eyebrow">Account</p>
        <h2>{mode === 'login' ? 'Welcome back.' : 'Create your listener profile.'}</h2>
        <p>Your account unlocks publishing, subscriptions, playlists, and ratings.</p>
        <div className="segmented">
          <button className={mode === 'login' ? 'active' : ''} onClick={() => setMode('login')} type="button">Login</button>
          <button className={mode === 'register' ? 'active' : ''} onClick={() => setMode('register')} type="button">Register</button>
        </div>
      </div>
      <form className="panel form-card" onSubmit={submit}>
        {error && <div className="notice error">{error}</div>}
        {mode === 'register' && (
          <>
            <label>Full name<input minLength="2" required value={form.fullName} onChange={(event) => setForm({ ...form, fullName: event.target.value })} /></label>
            <label>Email<input required type="email" value={form.email} onChange={(event) => setForm({ ...form, email: event.target.value })} /></label>
            <label>Bio<textarea value={form.bio} onChange={(event) => setForm({ ...form, bio: event.target.value })} /></label>
          </>
        )}
        <label>Username<input minLength="3" required value={form.userName} onChange={(event) => setForm({ ...form, userName: event.target.value })} /></label>
        <label>Password<input minLength="6" required type="password" value={form.password} onChange={(event) => setForm({ ...form, password: event.target.value })} /></label>
        <button className="button primary" disabled={pending} type="submit">{pending ? 'Working...' : mode === 'login' ? 'Login' : 'Register'}</button>
      </form>
    </section>
  );
}

function DashboardView({ episodes, playlists, subscriptions, user }) {
  if (!user) {
    return <section className="panel"><EmptyState text="Login to view your subscriptions and profile." /></section>;
  }

  return (
    <section className="dashboard-grid">
      <article className="panel profile-card">
        <img alt="" src={user.profileImage || fallbackImage(user.fullName || user.userName)} />
        <h2>{user.fullName || user.userName}</h2>
        <p>@{user.userName}</p>
        <p>{user.bio || 'No bio yet.'}</p>
      </article>
      <article className="panel">
        <p className="eyebrow">Subscriptions</p>
        <h2>{subscriptions.length}</h2>
        {subscriptions.length === 0 ? <EmptyState text="No subscriptions yet." /> : subscriptions.map((subscription) => (
          <div className="compact-row" key={subscription.id || subscription.podcastId}>
            <strong>{subscription.podcastTitle || `Podcast #${subscription.podcastId}`}</strong>
            <small>{formatDate(subscription.subscribedAt)}</small>
          </div>
        ))}
      </article>
      <article className="panel">
        <p className="eyebrow">Playlists</p>
        <h2>{playlists.length}</h2>
        {playlists.slice(0, 8).map((playlist) => (
          <div className="compact-row" key={playlist.id}>
            <strong>{playlist.name}</strong>
            <small>{playlist.itemCount || 0} items | {playlist.ownerName}</small>
          </div>
        ))}
      </article>
      <article className="panel">
        <p className="eyebrow">Recent episodes</p>
        <EpisodeList episodes={episodes.slice(0, 5)} />
      </article>
    </section>
  );
}

function CreateView({ isSignedIn, onCreateEpisode, onCreatePlaylist, onCreatePodcast, podcasts }) {
  if (!isSignedIn) {
    return <section className="panel"><EmptyState text="Login first to create podcasts, episodes, and playlists." /></section>;
  }

  return (
    <section className="create-grid">
      <PodcastForm onSubmit={onCreatePodcast} />
      <EpisodeForm onSubmit={onCreateEpisode} podcasts={podcasts} />
      <PlaylistForm onSubmit={onCreatePlaylist} />
    </section>
  );
}

function PodcastForm({ onSubmit }) {
  async function submit(event) {
    event.preventDefault();
    const form = event.currentTarget;
    const data = new FormData(form);
    data.set('Privacy', Number(data.get('Privacy')));
    await onSubmit(data);
    form.reset();
  }

  return (
    <form className="panel form-card" onSubmit={submit}>
      <p className="eyebrow">Create</p>
      <h2>Podcast</h2>
      <label>Title<input minLength="3" name="Title" required /></label>
      <label>Category<input name="Category" /></label>
      <label>Description<textarea name="Description" /></label>
      <label>Cover image<input accept="image/*" name="ImageFile" type="file" /></label>
      <PrivacySelect />
      <button className="button primary" type="submit">Create podcast</button>
    </form>
  );
}

function EpisodeForm({ onSubmit, podcasts }) {
  async function submit(event) {
    event.preventDefault();
    const form = event.currentTarget;
    const data = new FormData(form);
    data.set('PodcastId', Number(data.get('PodcastId')));
    data.set('Privacy', Number(data.get('Privacy')));
    await onSubmit(data);
    form.reset();
  }

  return (
    <form className="panel form-card" onSubmit={submit}>
      <p className="eyebrow">Publish</p>
      <h2>Episode</h2>
      <label>Podcast<select name="PodcastId" required>{podcasts.map((podcast) => <option key={podcast.id} value={podcast.id}>{podcast.title}</option>)}</select></label>
      <label>Title<input minLength="3" name="Title" required /></label>
      <label>Description<textarea name="Description" /></label>
      <label>Audio file<input accept="audio/*" name="AudioFile" required type="file" /></label>
      <label>Episode image<input accept="image/*" name="ImageFile" type="file" /></label>
      <PrivacySelect />
      <button className="button primary" disabled={podcasts.length === 0} type="submit">Create episode</button>
    </form>
  );
}

function PlaylistForm({ onSubmit }) {
  const [form, setForm] = useState({ name: '', description: '', privacy: 0 });

  async function submit(event) {
    event.preventDefault();
    await onSubmit({ ...form, privacy: Number(form.privacy) });
    setForm({ name: '', description: '', privacy: 0 });
  }

  return (
    <form className="panel form-card" onSubmit={submit}>
      <p className="eyebrow">Curate</p>
      <h2>Playlist</h2>
      <label>Name<input minLength="1" required value={form.name} onChange={(event) => setForm({ ...form, name: event.target.value })} /></label>
      <label>Description<textarea value={form.description} onChange={(event) => setForm({ ...form, description: event.target.value })} /></label>
      <label>Privacy<select value={form.privacy} onChange={(event) => setForm({ ...form, privacy: event.target.value })}><option value="0">Public</option><option value="1">Private</option></select></label>
      <button className="button primary" type="submit">Create playlist</button>
    </form>
  );
}

function RatingForm({ disabled, onSubmit }) {
  const [rating, setRating] = useState(5);
  const [review, setReview] = useState('');

  async function submit(event) {
    event.preventDefault();
    await onSubmit({ rating: Number(rating), review });
    setReview('');
  }

  return (
    <form className="rating-form" onSubmit={submit}>
      <label>Rate this podcast<select disabled={disabled} value={rating} onChange={(event) => setRating(event.target.value)}><option value="5">5 stars</option><option value="4">4 stars</option><option value="3">3 stars</option><option value="2">2 stars</option><option value="1">1 star</option></select></label>
      <label>Review<input disabled={disabled} placeholder={disabled ? 'Login to rate' : 'Short review'} value={review} onChange={(event) => setReview(event.target.value)} /></label>
      <button className="button" disabled={disabled} type="submit">Save rating</button>
    </form>
  );
}

function PrivacySelect() {
  return (
    <label>Privacy<select defaultValue="0" name="Privacy"><option value="0">Public</option><option value="1">Private</option></select></label>
  );
}

function EpisodeList({ episodes }) {
  if (!episodes?.length) return <EmptyState text="No episodes found." />;

  return episodes.map((episode) => (
    <article className="episode-card" key={episode.id}>
      <img alt="" src={episode.imageUrl || fallbackImage(episode.title)} />
      <div>
        <strong>{episode.title}</strong>
        <p>{episode.description || episode.podcastTitle}</p>
        <small>{episode.podcastTitle} | {formatDate(episode.publishedAt)}</small>
        {episode.audioUrl && <audio controls src={episode.audioUrl} />}
      </div>
    </article>
  ));
}

function EmptyState({ text }) {
  return <p className="empty-state">{text}</p>;
}

function fallbackImage(seed) {
  return `https://api.dicebear.com/9.x/shapes/svg?seed=${encodeURIComponent(seed || 'podcast')}&backgroundColor=151b2d,27324f,ed6a5a`;
}

export default App;
