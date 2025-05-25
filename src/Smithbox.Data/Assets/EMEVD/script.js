const Default = REST.Default;
const End = REST.End;
const Restart = REST.Restart;

var _event = void 0;
var _codeblock = void 0;
var _skips = void 0;

function Event(id, restBehavior, instructions) {
    var evt = new EVENT();
    evt.ID = id;
    evt.RestBehavior = restBehavior;

    _labels = {};
    _skips = {};
    _event = evt;
    instructions.apply(this, _GetArgs(instructions));
    if (_skips.length > 0) {
        throw new Error(`Reserved skips in Event ${id} have not been filled. Unfilled skips: ${JSON.stringify(_skips)}`);
    }
    _event = void 0;
    _skips = void 0;
    _labels = void 0;

    EVD.Events.Add(evt);
    return evt;
}

function _GetArgs(func) {
    var start = func.toString().indexOf("(");
    var end = func.toString().indexOf(")");
    var args = func.toString().substring(start, end).replace("(", "").replace(")", "");
    return args.split(/\s*,\s*/).map(arg => arg);
}

function _Instruction(bank, index, args) {

    if (_codeblock) {
        _codeblock.instructions.push(Array.from(arguments));
        return;
    }

    if (_event) {
        var layer = void 0;
        if (args.length) {
            var lastArg = args.pop();
            if (lastArg.layerValue) {
                layer = lastArg.layerValue;
            } else {
                args.push(lastArg);
            }
        }

        if (layer) {
            return Scripter.MakeInstruction(_event, bank, index, layer, hostArray(args));
        } else {
            return Scripter.MakeInstruction(_event, bank, index, hostArray(args));
        }
    }
}

function _ReserveSkip(id) {
    _skips[id] = _event.Instructions.Count;
    // Arbitrary, but checked later as a loose failsafe
    return 99;
}

function _FillSkip(id) {
    var index = id in _skips ? _skips[id] : -1;
    delete _skips[id];
    Scripter.FillSkipPlaceholder(_event, index);
}

function hostArray(args) {
    var argOut = $$$_host.newArr(args.length);
    for (var i = 0; i < args.length; i++) {
        argOut[i] = args[i];
    }
    return argOut;
}

function $LAYERS(...args) {
    var layer = 0;
    for (var i = 0; i < args.length; i++)
        layer |= 1 << args[i];
    return { layerValue: layer };
}

// Utility function
function floatArg(num) {
    return Scripter.ConvertFloatToIntBytes(num);
}

class CodeBlock {
    instructions = [];

    constructor(func) {
        _codeblock = this;
        func();
        _codeblock = void 0;
    }

    get length() { return this.instructions.length; }

    Exec = () => this.instructions.forEach(ins => _Instruction(ins[0], ins[1], ins[2]));
}
