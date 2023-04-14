// Game proposed by Currarini and Morelli (2000)
// The following is by Matthew O. Jackson:
// https://web.stanford.edu/~jacksonm/netsurv.pdf
// "
// The game that Currarini and Morelli analyze is described as follows. 
// Players are ordered exogenously according to a function ρ : N → N .
// Without loss of generality assume that this is in the order of their labels, so player 1 moves first, then player 2 and so forth. 
// A player i announces the set of players with whom her or she is willing to be linked (ai ∈ 2N\{i}), and a payoff demand di ∈ IR. 
// The outcome of the game is then as follows. 
// The actions a = (a1, . . . , an) determine a network g(a) by requiring that a link ij is in g(a) if and only if j ∈ ai and i ∈ aj.
// However, the network that is eventually formed is determined by checking which components of g(a) are actually feasible in terms of the demands submitted.
// That is, if h ∈ C(g(a)), then h is actually formed if and only if ∑ i∈N(h) di ≤ v(h).
// In cases where ∑ i∈N(h) di > v(h), the links in h are all deleted and the players in N (h) are left without any links.
// "