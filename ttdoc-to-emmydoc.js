function getdocs() {
    let output = ''
    let moduleName = document.getElementsByTagName('code')[0].textContent
    let moduleDesc = document.getElementById('content').getElementsByTagName('p')[0].textContent
    output += '---@type ' + moduleName + '\n'
    output += `${moduleName} = ${moduleName}\n`
    output += `---@class ${moduleName} ${moduleDesc}\n`

    let tables = document.getElementsByClassName('function')
    for (const table of tables) {
        let titles = table.getElementsByTagName('dt')
        let descriptions = table.getElementsByTagName('dd')
        for (let i = 0; i < titles.length; i++) {
            let title = titles[i].children[0].getAttribute('name').split(':').pop()
            let descriptionElement = descriptions[i]
            let description = descriptionElement.textContent;
            let descEnd = description.search(/Returns:|Parameters:|New feature:/gm)
            if(descEnd !== -1)
                description = description.substring(0, descEnd)
            var descriptionFiltered = "";
            for(let j = 0; j < description.length; j++ ) 
                if(!(description[j] == '\n'))
                    descriptionFiltered += description[j];
                else
                    descriptionFiltered += ' '
            description = descriptionFiltered.trim()

            let parameters = []
            let uls = descriptionElement.getElementsByTagName('ul')
            let isField = uls.length === 0
            if(uls.length > 0) {
                let parameterElements = descriptionElement.getElementsByTagName('ul')[descriptionElement.getElementsByTagName('ul').length - 1].getElementsByTagName('li')
                for (let j = 0; j < parameterElements.length; j++) {
                    const parameterElement = parameterElements[j]
                    let name = parameterElement.getElementsByClassName('parameter')[0].textContent
    
                    let typeElements = parameterElement.getElementsByClassName('type')
                    let type = null
                    if(typeElements.length !== 0) {
                        type = typeElements[0].textContent
                    }
    
                    let optionalElements = parameterElement.getElementsByTagName('em')
                    let optional = optionalElements.length !== 0
                    
                    let paramText = name
                    
                    if(optional)
                        paramText += '?'
                    paramText += ': '
    
                    if(type === null)
                        paramText += 'any'
                    else
                        paramText += type
                    parameters.push(paramText)
                }
            }

            let returnsHeader = descriptionElement.getElementsByTagName('h3')[1]
            let returnsAny = false
            if(returnsHeader !== undefined)
                returnsAny = descriptionElement.getElementsByTagName('h3')[1].textContent === 'Returns:'
            let returnType = null
            if(returnsAny) {
                let returnTypeTag = descriptionElement.getElementsByTagName('ol')[0].getElementsByClassName('types')
                if(returnTypeTag.length !== 0)
                    returnType = returnTypeTag[0].textContent
                else
                    returnsAny = false
            }

            //write the emmydocs line
            let line = '---@field '
            line += title
            if (!isField) {
                line += ' fun('
                line += parameters.join(', ')
                line += ')'
                if(returnsAny)
                    line += ': ' + returnType
            }
            else
                line += ' any'

            line += ' ' + description
            output += line + '\n'
        }
    }
    output = output.replace(/\bbool\b/g, 'boolean')
    output = output.replace(/anything/g, 'any')
    output = output.replace(/\bint\b|\binteger\b|\bfloat\b/g, 'number')
    console.log(output)
}
getdocs()